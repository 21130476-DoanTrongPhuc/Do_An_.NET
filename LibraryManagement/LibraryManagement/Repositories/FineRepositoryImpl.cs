using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Filters;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories;

public class FineRepositoryImpl : FineRepository
{
    // ==================== ADD ====================

    public async Task Add(LibraryDbContext context, CreateFineManagementRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (request.LoanDetailId <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.LoanDetailId), @"LoanDetailId không hợp lệ.");
        if (request.FineAmount <= 0)
            throw new ArgumentException(@"Số tiền phạt phải lớn hơn 0.", nameof(request));
        if (!Enum.IsDefined(request.ViolationType))
            throw new ArgumentException(@"Loại vi phạm không hợp lệ.", nameof(request));

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var loanDetailExists = await context.LoanDetails
                .AsNoTracking()
                .AnyAsync(ld => ld.Id == request.LoanDetailId);
            if (!loanDetailExists)
                throw new InvalidOperationException(
                    $"Không tìm thấy phiếu mượn chi tiết Id = {request.LoanDetailId}.");

            bool alreadyExists = await context.Fines.AnyAsync(f =>
                f.IdLoanDetail == request.LoanDetailId &&
                f.ViolationType == request.ViolationType &&
                f.Status != FineStatus.Cancelled);
            if (alreadyExists)
                throw new InvalidOperationException("Đã tồn tại phiếu phạt cùng loại cho phiếu mượn này.");

            // Insert mã tạm → cập nhật mã chính thức trong cùng transaction
            var fine = new Fines
            {
                FineCode      = $"TMP{Guid.NewGuid():N}".ToUpper(),  // full Guid, không cắt
                IdLoanDetail  = request.LoanDetailId,
                CreatedAt     = DateTime.Now,
                ViolationType = request.ViolationType,
                AmountVnd     = request.FineAmount,
                Description   = request.Note?.Trim(),
                Status        = FineStatus.Unpaid
            };
            context.Fines.Add(fine);
            await context.SaveChangesAsync();

            fine.FineCode = $"FC{fine.Id:D6}";

            context.FineAuditLogs.Add(new FineAuditLogs
            {
                IdFine       = fine.Id,
                Action       = FineAuditAction.Created,
                BeforeAmount = 0,
                AfterAmount  = request.FineAmount,
                Reason       = request.Note?.Trim(),
                At           = DateTime.Now
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ==================== UPDATE ====================

    public async Task Update(LibraryDbContext context, UpdateFineManagementRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (request.Id <= 0)
            throw new ArgumentException(@"Id phiếu phạt không hợp lệ.", nameof(request));
        if (request.LoanDetailId <= 0)
            throw new ArgumentException(@"LoanDetailId không hợp lệ.", nameof(request));
        if (request.FineAmount <= 0)
            throw new ArgumentException(@"Số tiền phạt phải > 0.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Note))
            throw new ArgumentException(@"Lý do không được rỗng.", nameof(request));
        if (!Enum.IsDefined(request.ViolationType))
            throw new ArgumentException(@"Loại vi phạm không hợp lệ.", nameof(request));

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var fine = await context.Fines
                .Include(f => f.FinePayments)
                .FirstOrDefaultAsync(f => f.Id == request.Id)
                ?? throw new InvalidOperationException($"Không tìm thấy phiếu phạt Id = {request.Id}.");

            if (fine.Status == FineStatus.Cancelled)
                throw new InvalidOperationException("Không thể sửa phiếu đã hủy.");

            if (fine.Status == FineStatus.Paid)
                throw new InvalidOperationException("Không thể sửa phiếu đã thanh toán đầy đủ.");

            if (fine.IdLoanDetail != request.LoanDetailId)
            {
                bool loanDetailExists = await context.LoanDetails
                    .AsNoTracking()
                    .AnyAsync(ld => ld.Id == request.LoanDetailId);
                if (!loanDetailExists)
                    throw new InvalidOperationException(
                        $"Không tìm thấy LoanDetail Id = {request.LoanDetailId}.");
            }

            // ⭐ Check trùng loại (loại trừ chính nó)
            bool duplicated = await context.Fines.AnyAsync(f =>
                f.Id != request.Id &&
                f.IdLoanDetail == request.LoanDetailId &&
                f.ViolationType == request.ViolationType &&
                f.Status != FineStatus.Cancelled);
            if (duplicated)
                throw new InvalidOperationException(
                    "Đã tồn tại phiếu phạt cùng loại cho phiếu mượn này.");

            int paidVnd = fine.FinePayments.Sum(p => p.AmountVnd);
            int newAmount = request.FineAmount;

            if (newAmount < paidVnd)
                throw new InvalidOperationException(
                    $"Số tiền phạt không được nhỏ hơn số đã trả ({paidVnd:N0} đ).");

            int oldAmount = fine.AmountVnd;
            int oldLoanDetailId = fine.IdLoanDetail;
            ViolationType oldType = fine.ViolationType;
            string? oldNote = fine.Description;

            fine.IdLoanDetail  = request.LoanDetailId;
            fine.ViolationType = request.ViolationType;
            fine.AmountVnd     = newAmount;
            fine.Description   = request.Note.Trim();

            if (newAmount > paidVnd)
                fine.Status = paidVnd > 0 ? FineStatus.Partial : FineStatus.Unpaid;

            var changes = new List<string>();
            if (oldLoanDetailId != request.LoanDetailId)
                changes.Add($"LoanDetail: {oldLoanDetailId} → {request.LoanDetailId}");
            if (oldType != request.ViolationType)
                changes.Add($"Loại: {oldType} → {request.ViolationType}");
            if (oldAmount != newAmount)
                changes.Add($"Tiền: {oldAmount:N0} → {newAmount:N0} đ");
            if (oldNote != fine.Description)
                changes.Add("Lý do: đã thay đổi");

            context.FineAuditLogs.Add(new FineAuditLogs
            {
                IdFine       = fine.Id,
                Action       = FineAuditAction.Adjusted,
                BeforeAmount = oldAmount,
                AfterAmount  = newAmount,
                Reason       = changes.Count > 0
                                 ? string.Join("; ", changes)
                                 : "Chỉnh sửa thông tin phiếu phạt",
                At           = DateTime.Now
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ==================== ADJUST AMOUNT ====================

    public async Task AdjustFineAmount(
        LibraryDbContext context,
        int fineId,
        int fineAmount,
        string note)
    {
        if (fineId <= 0)
            throw new ArgumentException(@"Id phiếu phạt không hợp lệ.", nameof(fineId));
        if (fineAmount <= 0)
            throw new ArgumentException(@"Số tiền phạt phải lớn hơn 0.", nameof(fineAmount));
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException(@"Lý do điều chỉnh không được rỗng.", nameof(note));

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var fine = await context.Fines
                .Include(f => f.FinePayments)
                .FirstOrDefaultAsync(f => f.Id == fineId)
                ?? throw new InvalidOperationException($"Không tìm thấy phiếu phạt Id = {fineId}.");

            if (fine.Status == FineStatus.Cancelled)
                throw new InvalidOperationException("Không thể điều chỉnh phiếu đã hủy.");

            if (fine.Status == FineStatus.Paid)
                throw new InvalidOperationException("Không thể điều chỉnh phiếu đã thanh toán đầy đủ.");

            int paidVnd = fine.FinePayments.Sum(p => p.AmountVnd);
            int newAmount = fineAmount;

            if (newAmount < paidVnd)
                throw new InvalidOperationException(
                    $"Số tiền phạt không được nhỏ hơn số đã trả ({paidVnd:N0} đ).");

            if (newAmount == fine.AmountVnd)
                throw new InvalidOperationException("Mức phạt mới phải khác mức hiện tại.");
            
            int oldAmount = fine.AmountVnd;
            fine.AmountVnd = newAmount;

            if (newAmount > paidVnd)
                fine.Status = paidVnd > 0 ? FineStatus.Partial : FineStatus.Unpaid;

            context.FineAuditLogs.Add(new FineAuditLogs
            {
                IdFine       = fine.Id,
                Action       = FineAuditAction.Adjusted,
                BeforeAmount = oldAmount,
                AfterAmount  = newAmount,
                Reason       = note.Trim(),
                At           = DateTime.Now
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ==================== DELETE / CANCEL ====================

    public async Task Delete(LibraryDbContext context, int fineId, string note)
    {
        if (fineId <= 0)
            throw new ArgumentException(@"Id không hợp lệ.", nameof(fineId));
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException(@"Lý do không được rỗng.", nameof(note));

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var fine = await context.Fines
                .Include(f => f.FinePayments)
                .FirstOrDefaultAsync(f => f.Id == fineId)
                ?? throw new InvalidOperationException($"Không tìm thấy phiếu phạt Id = {fineId}.");

            if (fine.Status == FineStatus.Cancelled)
                throw new InvalidOperationException("Phiếu đã hủy.");

            if (fine.FinePayments.Any())
                throw new InvalidOperationException(
                    "Không thể hủy phiếu đã có thanh toán. Hãy hoàn tiền trước.");
            
            int oldAmount = fine.AmountVnd;
            fine.Status = FineStatus.Cancelled;
            // ⭐ KHÔNG zero hóa AmountVnd — giữ dấu vết số tiền gốc

            context.FineAuditLogs.Add(new FineAuditLogs
            {
                IdFine       = fine.Id,
                Action       = FineAuditAction.Cancelled,
                BeforeAmount = oldAmount,
                AfterAmount  = 0,         
                Reason       = note.Trim(),
                At           = DateTime.Now
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ==================== GET LIST (không phân trang) ====================

    public async Task<List<FineManagementInfo>> GetFineManagementInfo(
        LibraryDbContext context,
        FineManagementFilter filter)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var query = context.Fines.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            string kw = filter.Keyword.Trim().ToLower();
            query = query.Where(f =>
                f.FineCode.ToLower().Contains(kw) ||
                (f.LoanDetail != null &&
                    (f.LoanDetail.Loan.Reader.UserCode.ToLower().Contains(kw) ||
                     f.LoanDetail.Loan.Reader.FullName.ToLower().Contains(kw) ||
                     f.LoanDetail.Loan.LoanCode.ToLower().Contains(kw) ||
                     f.LoanDetail.BookCopy.CopyCode.ToLower().Contains(kw))));
        }

        if (filter.ViolationType.HasValue)
            query = query.Where(f => f.ViolationType == filter.ViolationType.Value);

        if (filter.FineStatus.HasValue)
            query = query.Where(f => f.Status == filter.FineStatus.Value);

        if (filter.StartDate.HasValue)
        {
            var from = filter.StartDate.Value.Date;
            query = query.Where(f => f.CreatedAt >= from);
        }
        if (filter.EndDate.HasValue)
        {
            var to = filter.EndDate.Value.Date.AddDays(1);
            query = query.Where(f => f.CreatedAt < to);
        }

        return await query
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FineManagementInfo
            {
                FineId      = f.Id,
                FineCode    = f.FineCode,
                LoanCode    = f.LoanDetail != null ? f.LoanDetail.Loan.LoanCode : null,
                ReaderCode  = f.LoanDetail != null ? f.LoanDetail.Loan.Reader.UserCode : "",
                ReaderName  = f.LoanDetail != null ? f.LoanDetail.Loan.Reader.FullName : "",
                BookTitle   = f.LoanDetail != null ? f.LoanDetail.BookCopy.Book.Title : null,
                CopyBarcode = f.LoanDetail != null ? f.LoanDetail.BookCopy.CopyCode : null,

                ViolationType = f.ViolationType,
                Description   = f.Description,
                CreatedAt     = f.CreatedAt,
                AmountVnd     = f.AmountVnd,
                PaidVnd       = f.FinePayments.Sum(p => (int?)p.AmountVnd) ?? 0,

                Status = f.Status
            })
            .ToListAsync();
    }

    // ==================== GET DETAIL ====================

    public async Task<FineDetailInfo?> GetFineDetail(LibraryDbContext context, int fineId)
    {
        var fine = await context.Fines
            .AsNoTracking()
            .Include(f => f.FinePayments)
            .Include(f => f.AuditLogs)
            .FirstOrDefaultAsync(f => f.Id == fineId);

        if (fine == null) return null;

        return new FineDetailInfo
        {
            FineId   = fine.Id,
            FineCode = fine.FineCode,
            Payments = fine.FinePayments
                .OrderBy(p => p.PaidAt)
                .Select((p, idx) => new FinePaymentInfo
                {
                    No            = idx + 1,
                    PaidAt        = p.PaidAt,
                    AmountVnd     = p.AmountVnd,
                    PaymentMethod = p.PaymentMethod,
                    Note          = p.Note,
                    PaymentCode   = p.PaymentCode
                }).ToList(),
            Audits = fine.AuditLogs
                .OrderByDescending(a => a.At)
                .Take(50)                       // ⭐ giới hạn 50 bản ghi gần nhất
                .Select(a => new FineAuditInfo
                {
                    At           = a.At,
                    Action       = a.Action,
                    BeforeAmount = a.BeforeAmount,
                    AfterAmount  = a.AfterAmount,
                    Reason       = a.Reason,
                }).ToList()
        };
    }
}
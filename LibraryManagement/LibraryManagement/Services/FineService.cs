using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Filters;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services;

public class FineService
{
    private readonly LibraryDbContext _context;
    private readonly FineRepository _fineRepository;

    public FineService(LibraryDbContext context, FineRepository fineRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _fineRepository = fineRepository ?? throw new ArgumentNullException(nameof(fineRepository));
    }

    // ==================== HELPER ====================

    private async Task<int> GetLibrarianIdAsync()
    {
        var librarian = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserType == UserType.Librarian);

        if (librarian == null)
            throw new InvalidOperationException("Không tìm thấy thủ thư trong hệ thống.");

        return librarian.Id;
    }

    // ==================== CREATE ====================

    public async Task CreateFineAsync(CreateFineManagementRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (request.LoanDetailId <= 0)
            throw new ArgumentException("LoanDetailId không hợp lệ.", nameof(request));
        if (request.FineAmount <= 0)
            throw new ArgumentException("Số tiền phạt phải lớn hơn 0.", nameof(request));
        if (!Enum.IsDefined(request.ViolationType))
            throw new ArgumentException("Loại vi phạm không hợp lệ.", nameof(request));

        await _fineRepository.Add(_context, request);
    }

    // ==================== UPDATE ====================

    public async Task UpdateFineAsync(UpdateFineManagementRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        await _fineRepository.Update(_context, request);
    }

    // ==================== ADJUST AMOUNT ====================

    public async Task AdjustFineAmountAsync(int fineId, int fineAmount, string note)
    {
        if (fineId <= 0)
            throw new ArgumentException("Id phiếu phạt không hợp lệ.", nameof(fineId));
        if (fineAmount <= 0)
            throw new ArgumentException("Số tiền phạt phải lớn hơn 0.", nameof(fineAmount));
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("Lý do điều chỉnh không được rỗng.", nameof(note));

        await _fineRepository.AdjustFineAmount(_context, fineId, fineAmount, note);
    }

    // ==================== CANCEL ====================

    public async Task CancelFineAsync(int fineId, string note)
    {
        if (fineId <= 0)
            throw new ArgumentException("Id phiếu phạt không hợp lệ.", nameof(fineId));
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("Lý do hủy không được rỗng.", nameof(note));

        await _fineRepository.Delete(_context, fineId, note);
    }

    // ==================== GET LIST ====================

    public async Task<List<FineManagementInfo>> GetFineManagementInfoAsync(
        FineManagementFilter filter)
    {
        filter ??= new FineManagementFilter();

        if (filter.StartDate.HasValue && filter.EndDate.HasValue
            && filter.StartDate.Value.Date > filter.EndDate.Value.Date)
        {
            (filter.StartDate, filter.EndDate) = (filter.EndDate, filter.StartDate);
        }

        return await _fineRepository.GetFineManagementInfo(_context, filter);
    }

    // ==================== PAY ====================

    public async Task PayFineAsync(
        int fineId,
        int amount,
        PaymentMethod paymentMethod,
        string? note)
    {
        if (fineId <= 0)
            throw new ArgumentException("Id phiếu phạt không hợp lệ.", nameof(fineId));
        if (amount <= 0)
            throw new ArgumentException("Số tiền thu phải > 0.", nameof(amount));
        if (!Enum.IsDefined(paymentMethod))
            throw new ArgumentException("Phương thức thanh toán không hợp lệ.", nameof(paymentMethod));

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // RowVersion được EF kiểm tra khi SaveChanges
            var fine = await _context.Fines
                .Include(f => f.FinePayments)
                .FirstOrDefaultAsync(f => f.Id == fineId)
                ?? throw new InvalidOperationException($"Không tìm thấy phiếu phạt Id = {fineId}.");

            if (fine.Status == FineStatus.Cancelled)
                throw new InvalidOperationException("Không thể thu tiền phiếu đã hủy.");

            if (fine.Status == FineStatus.Paid)
                throw new InvalidOperationException("Phiếu đã thanh toán đầy đủ.");

            int paidVnd   = fine.FinePayments.Sum(p => p.AmountVnd);
            int remaining = fine.AmountVnd - paidVnd;

            if (remaining <= 0)
                throw new InvalidOperationException("Phiếu không còn nợ.");

            if (amount > remaining)
                throw new InvalidOperationException(
                    $"Số tiền thu vượt quá số còn nợ ({remaining:N0} đ).");

            int librarianId = await GetLibrarianIdAsync();

            var payment = new FinePayments
            {
                IdFine        = fine.Id,
                PaidAt        = DateTime.Now,
                AmountVnd     = amount,
                PaymentMethod = paymentMethod,
                Note          = note?.Trim(),
                PaymentCode   = $"TMP{Guid.NewGuid():N}".ToUpper()
            };
            _context.FinePayments.Add(payment);
            await _context.SaveChangesAsync();

            payment.PaymentCode = $"PC{payment.Id:D6}";

            int newPaid = paidVnd + amount;
            int debtBefore = remaining;
            int debtAfter  = fine.AmountVnd - newPaid;

            fine.Status = newPaid >= fine.AmountVnd ? FineStatus.Paid : FineStatus.Partial;

            // ⭐ Audit: Before/After = số NỢ trước/sau khi thu
            _context.FineAuditLogs.Add(new FineAuditLogs
            {
                IdFine       = fine.Id,
                Action       = FineAuditAction.Paid,
                BeforeAmount = debtBefore,
                AfterAmount  = debtAfter,
                Reason       = $"Thu tiền ({paymentMethod}): {amount:N0} đ" +
                               (string.IsNullOrWhiteSpace(note) ? "" : $" — {note.Trim()}"),
                ActorUserId  = librarianId,
                At           = DateTime.Now
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException(
                "Phiếu phạt vừa được thay đổi bởi người khác. Vui lòng tải lại và thử lại.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ==================== GET DETAIL ====================

    public async Task<FineDetailInfo?> GetFineDetailAsync(int fineId)
    {
        if (fineId <= 0) return null;
        return await _fineRepository.GetFineDetail(_context, fineId);
    }
}
using LibraryManagement.Data;
using LibraryManagement.Enums;
using LibraryManagement.Helpers;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services;

#region DTOs Nghiệp vụ Mượn sách (Phần 2)

public class ReaderBorrowContextDto
{
    public int ReaderId { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserStatus AccountStatus { get; set; }

    public string CardCode { get; set; } = string.Empty;
    public DateTime? CardExpiresAt { get; set; }
    public ReaderCardStatus CardStatus { get; set; } = ReaderCardStatus.Missing;

    public int ActiveBorrowedCount { get; set; }
    public int OverdueCount { get; set; }
    public int MaxActiveCopies { get; set; }
    public int UnpaidFineVnd { get; set; }

    public bool IsEligible { get; set; }
    public List<string> ValidationMessages { get; set; } = new();
}

public class BorrowCartItemDto
{
    public int BookCopyId { get; set; }
    public int BookId { get; set; }
    public string CopyCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public BookCondition PhysicalCondition { get; set; } = BookCondition.Good;
    public int MaxLoanDays { get; set; }
    public DateTime DueAt { get; set; }
}

public class LoanSummaryDto
{
    public int LoanId { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public int ReaderId { get; set; }
    public string ReaderCode { get; set; } = string.Empty;
    public string ReaderName { get; set; } = string.Empty;
    public string CardCode { get; set; } = string.Empty;
    public DateTime BorrowedAt { get; set; }
    public int TotalCopies { get; set; }
    public int ReturnedCopies { get; set; }
    public string Status { get; set; } = string.Empty; // Đang mượn | Đã trả một phần | Đã trả đủ | Quá hạn
    public string Note { get; set; } = string.Empty;
}

public class LoanDetailItemDto
{
    public int LoanDetailId { get; set; }
    public int LoanId { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public int BookCopyId { get; set; }
    public string CopyCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime BorrowedAt { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public BookCondition PhysicalConditionBefore { get; set; } 
    public BookCondition? PhysicalConditionAfter { get; set; }
    public int RenewalCount { get; set; }
    public string ItemStatus { get; set; } = string.Empty; // Đang mượn | Quá hạn | Đã trả
    public string? Note { get; set; }
}

public class LoanReceiptDto
{
    public string LoanCode { get; set; } = string.Empty;
    public DateTime BorrowedAt { get; set; }
    public string ReaderCode { get; set; } = string.Empty;
    public string ReaderName { get; set; } = string.Empty;
    public string CardCode { get; set; } = string.Empty;
    public string ReaderPhone { get; set; } = string.Empty;
    public string LibrarianName { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public List<LoanDetailItemDto> Items { get; set; } = new();
}

#endregion

public class LoanManagementService
{
    private const int MaxAllowedUnpaidFineVnd = 0;

    private static string GetCurrentLibrarianDisplay(LibraryDbContext db)
    {
        if (Session.CurrentLibrarianId <= 0) return "Thủ thư hệ thống";
        var lib = db.Users
            .Where(u => u.Id == Session.CurrentLibrarianId)
            .Select(u => new { u.UserCode, u.FullName })
            .FirstOrDefault();
        return lib != null ? $"{lib.FullName} ({lib.UserCode})" : $"NV#{Session.CurrentLibrarianId}";
    }

    // ==================== 2.1 & 2.2. QUÉT THẺ & KIỂM TRA ĐIỀU KIỆN MƯỢN ====================

    public ReaderBorrowContextDto? FindAndValidateReader(string keyword, int pendingCartCount = 0)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return null;
        string kw = keyword.Trim().ToLower();

        using var db = new LibraryDbContext();

        var matchedCard = db.ReaderCards
            .Include(c => c.Reader)
            .FirstOrDefault(c => c.CardCode.ToLower() == kw);

        Users? reader = matchedCard?.Reader ?? db.Users.FirstOrDefault(u =>
            u.UserType == UserType.Reader &&
            (u.UserCode.ToLower() == kw ||
             (u.Phone != null && u.Phone == keyword.Trim()) ||
             u.FullName.ToLower().Contains(kw)));

        if (reader == null) return null;

        var card = matchedCard ?? db.ReaderCards
            .Where(c => c.IdReader == reader.Id)
            .OrderByDescending(c => c.Status == ReaderCardStatus.Active)
            .ThenByDescending(c => c.ExpiresAt)
            .FirstOrDefault();

        var activeDetails = db.LoanDetails
            .Where(d => d.Loan.IdReader == reader.Id && d.ReturnedAt == null)
            .Select(d => new { d.DueAt })
            .ToList();

        int activeCount = activeDetails.Count;
        int overdueCount = activeDetails.Count(d => d.DueAt.Date < DateTime.Today);

        // Chỉ Select các cột tồn tại trong bảng Fines để tránh lỗi ánh xạ cột Status
        var fineBalances = db.Fines
            .Where(f => f.LoanDetail.Loan.IdReader == reader.Id)
            .Select(f => new
            {
                f.AmountVnd,
                PaidVnd = f.FinePayments.Sum(p => (int?)p.AmountVnd) ?? 0
            })
            .ToList();

        int unpaidFine = fineBalances.Sum(f => Math.Max(0, f.AmountVnd - f.PaidVnd));
        int maxCopies = reader.MaxActiveCopies ?? 3;

        var ctx = new ReaderBorrowContextDto
        {
            ReaderId = reader.Id,
            UserCode = reader.UserCode,
            FullName = reader.FullName,
            Phone = reader.Phone,
            AccountStatus = reader.Status,
            CardCode = card?.CardCode ?? "Chưa cấp thẻ",
            CardExpiresAt = card?.ExpiresAt,
            CardStatus = card?.Status ??  ReaderCardStatus.Missing,
            ActiveBorrowedCount = activeCount,
            OverdueCount = overdueCount,
            MaxActiveCopies = maxCopies,
            UnpaidFineVnd = unpaidFine
        };

        // 1. Thẻ thư viện còn hiệu lực và không bị khóa
        if (card == null)
        {
            ctx.ValidationMessages.Add("Độc giả chưa được cấp thẻ thư viện.");
        }
        else
        {
            if (card.Status == ReaderCardStatus.Locked)
                ctx.ValidationMessages.Add($"Thẻ thư viện ({card.CardCode}) đang bị KHÓA.");
            if (card.Status == ReaderCardStatus.Expired || card.ExpiresAt.Date < DateTime.Today)
                ctx.ValidationMessages.Add($"Thẻ thư viện ({card.CardCode}) đã HẾT HẠN ({card.ExpiresAt:dd/MM/yyyy}).");
        }

        // 2. Tài khoản độc giả không bị khóa quyền mượn
        if (reader.Status != UserStatus.Active)
            ctx.ValidationMessages.Add("Tài khoản độc giả đang bị khóa quyền mượn.");

        // 3. Độc giả không có khoản nợ phạt vượt mức cho phép
        if (unpaidFine > MaxAllowedUnpaidFineVnd)
            ctx.ValidationMessages.Add($"Độc giả đang nợ tiền phạt chưa thanh toán: {unpaidFine:N0} VNĐ.");

        // 4. Độc giả chưa vượt quá giới hạn số lượng sách được mượn
        if (activeCount + pendingCartCount > maxCopies)
            ctx.ValidationMessages.Add($"Vượt giới hạn số sách được mượn ({activeCount + pendingCartCount}/{maxCopies} cuốn).");
        else if (pendingCartCount == 0 && activeCount >= maxCopies)
            ctx.ValidationMessages.Add($"Độc giả đã mượn tối đa {activeCount}/{maxCopies} cuốn.");

        // 5. Quy định hạn chế mượn: đang giữ sách quá hạn
        if (overdueCount > 0)
            ctx.ValidationMessages.Add($"Độc giả đang giữ {overdueCount} cuốn sách QUÁ HẠN chưa trả.");

        ctx.IsEligible = ctx.ValidationMessages.Count == 0;
        return ctx;
    }

    public BorrowCartItemDto ValidateAndPrepareCopy(string copyCode, int readerId, IEnumerable<BorrowCartItemDto> currentCart)
    {
        if (string.IsNullOrWhiteSpace(copyCode))
            throw new InvalidOperationException("Vui lòng nhập hoặc quét mã vạch bản sách.");

        string code = copyCode.Trim().ToUpper();
        var cartList = currentCart.ToList();

        if (cartList.Any(x => x.CopyCode.Equals(code, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Bản sách '{code}' đã có trong danh sách mượn.");

        using var db = new LibraryDbContext();

        var copy = db.BookCopies
            .Include(c => c.Book)
                .ThenInclude(b => b.Category)
            .FirstOrDefault(c => c.CopyCode.ToUpper() == code)
            ?? throw new InvalidOperationException($"Không tìm thấy bản sách có mã '{code}'.");

        // 6. Bản sách đang ở trạng thái có sẵn và đủ điều kiện lưu thông
        if (!copy.Book.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Đầu sách '{copy.Book.Title}' đã ngừng lưu hành.");

        bool isReservedForCurrentReader = copy.CirculationStatus == CirculationStatus.Reserved &&
            db.Reservations.Any(r => r.IdBookCopy == copy.Id && r.IdReader == readerId && r.Status == ReservationStatus.ReadyForPickup);

        if (copy.CirculationStatus != CirculationStatus.Available && !isReservedForCurrentReader)
            throw new InvalidOperationException($"Bản sách '{code}' không ở trạng thái có sẵn (Hiện tại: {copy.CirculationStatus}).");

        if (copy.PhysicalCondition == BookCondition.Damaged)
            throw new InvalidOperationException($"Bản sách '{code}' đang hư hỏng (Damaged), không đủ điều kiện lưu thông.");

        // Quy định hạn chế: không mượn trùng 2 bản của cùng 1 đầu sách
        if (cartList.Any(x => x.BookId == copy.IdBook))
            throw new InvalidOperationException($"Phiếu mượn đã có một bản sao của đầu sách '{copy.Book.Title}'.");

        bool alreadyHoldingTitle = db.LoanDetails
            .Any(d => d.Loan.IdReader == readerId && d.ReturnedAt == null && d.BookCopy.IdBook == copy.IdBook);

        if (alreadyHoldingTitle)
            throw new InvalidOperationException($"Độc giả đang mượn một bản sao khác của đầu sách '{copy.Book.Title}'.");

        int loanDays = copy.Book.Category?.MaxLoanDays ?? 14;
        return new BorrowCartItemDto
        {
            BookCopyId = copy.Id,
            BookId = copy.IdBook,
            CopyCode = copy.CopyCode,
            Title = copy.Book.Title,
            Author = copy.Book.Author,
            PhysicalCondition = copy.PhysicalCondition,
            MaxLoanDays = loanDays,
            DueAt = DateTime.Today.AddDays(loanDays)
        };
    }

    // ==================== 2.3. XÁC NHẬN GIAO DỊCH MƯỢN (TRANSACTION) ====================

    public Loans CreateLoanTransaction(int readerId, List<BorrowCartItemDto> items, string? note)
    {
        if (readerId <= 0)
            throw new InvalidOperationException("Vui lòng chọn độc giả trước khi lập phiếu mượn.");
        if (items == null || items.Count == 0)
            throw new InvalidOperationException("Phiếu mượn phải có ít nhất 1 bản sách.");

        using var db = new LibraryDbContext();
        var userCode = db.Users.Where(u => u.Id == readerId).Select(u => u.UserCode).FirstOrDefault() ?? "";
        var readerCheck = FindAndValidateReader(userCode, items.Count)
            ?? throw new InvalidOperationException("Không tìm thấy độc giả.");

        if (!readerCheck.IsEligible)
            throw new InvalidOperationException("Độc giả không đủ điều kiện mượn:\n- " +
                string.Join("\n- ", readerCheck.ValidationMessages));

        using var transaction = db.Database.BeginTransaction();
        try
        {
            DateTime now = DateTime.Now;
            string nowStr = now.ToString("yyyy-MM-dd HH:mm:ss");
            string librarianInfo = GetCurrentLibrarianDisplay(db);

            int nextId = (db.Loans.Max(l => (int?)l.Id) ?? 0) + 1;
            string loanCode = $"LN{nextId:D4}";
            while (db.Loans.Any(l => l.LoanCode == loanCode))
            {
                nextId++;
                loanCode = $"LN{nextId:D4}";
            }

            var loan = new Loans
            {
                LoanCode = loanCode,
                IdReader = readerId,
                BorrowedAt = now,
                Note = string.IsNullOrWhiteSpace(note)
                    ? $"Thủ thư: {librarianInfo}"
                    : $"{note.Trim()} | Thủ thư: {librarianInfo}"
            };

            db.Loans.Add(loan);
            db.SaveChanges();

            // Kiểm tra lại trạng thái từng bản sách ngay trong Transaction
            foreach (var item in items)
            {
                if (item.DueAt.Date < now.Date)
                    throw new InvalidOperationException($"Hạn trả của '{item.CopyCode}' không hợp lệ.");

                var copy = db.BookCopies
                    .Include(c => c.Book)
                    .FirstOrDefault(c => c.Id == item.BookCopyId)
                    ?? throw new InvalidOperationException($"Không tìm thấy bản sách '{item.CopyCode}'.");

                var matchedReservation = db.Reservations.FirstOrDefault(r =>
                    r.IdBookCopy == copy.Id && r.IdReader == readerId && r.Status == ReservationStatus.ReadyForPickup);

                bool canLend = copy.Book.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)
                    && copy.PhysicalCondition != BookCondition.Damaged
                    && (copy.CirculationStatus == CirculationStatus.Available)
                        || (copy.CirculationStatus == CirculationStatus.Reserved && matchedReservation != null);

                if (!canLend)
                {
                    throw new InvalidOperationException(
                        $"Bản sách '{copy.CopyCode}' không còn sẵn sàng lưu thông (Trạng thái: {copy.CirculationStatus}).");
                }

                CirculationStatus oldStatus = copy.CirculationStatus;

                db.LoanDetails.Add(new LoanDetails
                {
                    IdLoan = loan.Id,
                    IdBookCopy = copy.Id,
                    DueAt = item.DueAt.Date,
                    ReturnedAt = null,
                    PhysicalConditionBefore = copy.PhysicalCondition,
                    PhysicalConditionAfter = null,
                    RenewalCount = 0,
                    Note = $"Thủ thư lập phiếu: {librarianInfo}"
                });

                if (matchedReservation != null)
                {
                    matchedReservation.Status = ReservationStatus.Completed;
                    matchedReservation.FulfilledAt = now;
                    matchedReservation.IdLoan = loan.Id;
                }

                copy.CirculationStatus = CirculationStatus.Borrowed;
                copy.UpdatedAt = now;

                db.BookCopyHistories.Add(new BookCopyHistory
                {
                    IdBookCopy = copy.Id,
                    ChangedAt = nowStr,
                    ActionType = $"Cho mượn ({loanCode})",
                    OldStatus = oldStatus,
                    NewStatus = CirculationStatus.Borrowed,
                    OldCondition = copy.PhysicalCondition,
                    NewCondition = copy.PhysicalCondition,
                    Note = $"Độc giả: {readerCheck.FullName} ({readerCheck.UserCode}) | Thủ thư: {librarianInfo}"
                });
            }

            db.SaveChanges();
            transaction.Commit();
            return loan;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // ==================== 2.4. QUẢN LÝ PHIẾU MƯỢN ====================

    public List<LoanSummaryDto> SearchLoans(string? keyword, string statusFilter, DateTime? fromDate, DateTime? toDate)
    {
        using var db = new LibraryDbContext();
        var today = DateTime.Today;

        var query = db.Loans
            .Include(l => l.Reader)
                .ThenInclude(r => r.ReaderCards)
            .Include(l => l.LoanDetails)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(l => l.BorrowedAt >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(l => l.BorrowedAt < toDate.Value.Date.AddDays(1));

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string kw = keyword.Trim().ToLower();
            query = query.Where(l =>
                l.LoanCode.ToLower().Contains(kw) ||
                l.Reader.FullName.ToLower().Contains(kw) ||
                l.Reader.UserCode.ToLower().Contains(kw) ||
                l.Reader.ReaderCards.Any(c => c.CardCode.ToLower().Contains(kw)));
        }

        var list = query
            .OrderByDescending(l => l.BorrowedAt)
            .ThenByDescending(l => l.Id)
            .ToList()
            .Select(l =>
            {
                int total = l.LoanDetails.Count;
                int returned = l.LoanDetails.Count(d => d.ReturnedAt != null);
                int overdue = l.LoanDetails.Count(d => d.ReturnedAt == null && d.DueAt.Date < today);

                string status;
                if (total > 0 && returned == total)
                    status = "Đã trả đủ";
                else if (overdue > 0)
                    status = "Quá hạn";
                else if (returned > 0)
                    status = "Đã trả một phần";
                else
                    status = "Đang mượn";

                string cardCode = l.Reader.ReaderCards
                    .OrderByDescending(c => c.Status == ReaderCardStatus.Active)
                    .Select(c => c.CardCode)
                    .FirstOrDefault() ?? "—";

                return new LoanSummaryDto
                {
                    LoanId = l.Id,
                    LoanCode = l.LoanCode,
                    ReaderId = l.IdReader,
                    ReaderCode = l.Reader.UserCode,
                    ReaderName = l.Reader.FullName,
                    CardCode = cardCode,
                    BorrowedAt = l.BorrowedAt,
                    TotalCopies = total,
                    ReturnedCopies = returned,
                    Status = status,
                    Note = l.Note ?? ""
                };
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "Tất cả")
            list = list.Where(x => x.Status == statusFilter).ToList();

        return list;
    }

    public List<LoanDetailItemDto> GetLoanDetails(int loanId)
    {
        using var db = new LibraryDbContext();
        var today = DateTime.Today;

        return db.LoanDetails
            .Include(d => d.Loan)
            .Include(d => d.BookCopy)
                .ThenInclude(c => c.Book)
            .Where(d => d.IdLoan == loanId)
            .OrderBy(d => d.Id)
            .ToList()
            .Select(d => MapDetailDto(d, today))
            .ToList();
    }

    public List<LoanDetailItemDto> GetReaderBorrowHistory(int readerId)
    {
        using var db = new LibraryDbContext();
        var today = DateTime.Today;

        return db.LoanDetails
            .Include(d => d.Loan)
            .Include(d => d.BookCopy)
                .ThenInclude(c => c.Book)
            .Where(d => d.Loan.IdReader == readerId)
            .OrderByDescending(d => d.Loan.BorrowedAt)
            .ThenByDescending(d => d.Id)
            .ToList()
            .Select(d => MapDetailDto(d, today))
            .ToList();
    }

    private static LoanDetailItemDto MapDetailDto(LoanDetails d, DateTime today)
    {
        string status = d.ReturnedAt != null
            ? "Đã trả"
            : (d.DueAt.Date < today ? "Quá hạn" : "Đang mượn");

        return new LoanDetailItemDto
        {
            LoanDetailId = d.Id,
            LoanId = d.IdLoan,
            LoanCode = d.Loan.LoanCode,
            BookCopyId = d.IdBookCopy,
            CopyCode = d.BookCopy.CopyCode,
            Title = d.BookCopy.Book.Title,
            Author = d.BookCopy.Book.Author,
            BorrowedAt = d.Loan.BorrowedAt,
            DueAt = d.DueAt,
            ReturnedAt = d.ReturnedAt,
            PhysicalConditionBefore = d.PhysicalConditionBefore,
            PhysicalConditionAfter = d.PhysicalConditionAfter,
            RenewalCount = d.RenewalCount,
            ItemStatus = status,
            Note = d.Note
        };
    }

    public LoanReceiptDto GetLoanReceipt(int loanId)
    {
        using var db = new LibraryDbContext();
        var loan = db.Loans
            .Include(l => l.Reader)
                .ThenInclude(r => r.ReaderCards)
            .FirstOrDefault(l => l.Id == loanId)
            ?? throw new InvalidOperationException("Không tìm thấy phiếu mượn.");

        string cardCode = loan.Reader.ReaderCards
            .OrderByDescending(c => c.Status == ReaderCardStatus.Active)
            .Select(c => c.CardCode)
            .FirstOrDefault() ?? "—";

        return new LoanReceiptDto
        {
            LoanCode = loan.LoanCode,
            BorrowedAt = loan.BorrowedAt,
            ReaderCode = loan.Reader.UserCode,
            ReaderName = loan.Reader.FullName,
            CardCode = cardCode,
            ReaderPhone = loan.Reader.Phone ?? "—",
            LibrarianName = GetCurrentLibrarianDisplay(db),
            Note = loan.Note ?? "",
            Items = GetLoanDetails(loanId)
        };
    }

    /// <summary>
    /// Mục 2.4: Hỗ trợ trả từng cuốn sách trong cùng một phiếu mượn
    /// </summary>
    public void ReturnSingleLoanDetail(int loanDetailId, BookCondition conditionAfter, string? note)
    {
        if (!Enum.IsDefined(typeof(BookCondition), conditionAfter))
            conditionAfter = BookCondition.Good;

        using var db = new LibraryDbContext();
        using var transaction = db.Database.BeginTransaction();
        try
        {
            var detail = db.LoanDetails
                .Include(d => d.Loan)
                .Include(d => d.BookCopy)
                .FirstOrDefault(d => d.Id == loanDetailId)
                ?? throw new InvalidOperationException("Không tìm thấy chi tiết phiếu mượn.");

            if (detail.ReturnedAt != null)
                throw new InvalidOperationException($"Bản sách '{detail.BookCopy.CopyCode}' đã được trả trước đó.");

            DateTime now = DateTime.Now;
            string librarianInfo = GetCurrentLibrarianDisplay(db);

            detail.ReturnedAt = now;
            detail.PhysicalConditionAfter = conditionAfter;
            if (!string.IsNullOrWhiteSpace(note))
                detail.Note = string.IsNullOrWhiteSpace(detail.Note) ? note.Trim() : $"{detail.Note} | Trả lẻ: {note.Trim()}";

            var copy = detail.BookCopy;
            CirculationStatus oldStatus = copy.CirculationStatus;
            BookCondition oldCond = copy.PhysicalCondition;

            copy.PhysicalCondition = conditionAfter;
            copy.CirculationStatus = conditionAfter == BookCondition.Damaged ? CirculationStatus.Maintenance : CirculationStatus.Available;
            copy.UpdatedAt = now;

            db.BookCopyHistories.Add(new BookCopyHistory
            {
                IdBookCopy = copy.Id,
                ChangedAt = now.ToString("yyyy-MM-dd HH:mm:ss"),
                ActionType = $"Trả lẻ theo phiếu ({detail.Loan.LoanCode})",
                OldStatus = oldStatus,
                NewStatus = copy.CirculationStatus,
                OldCondition = oldCond,
                NewCondition = conditionAfter,
                Note = $"Thủ thư: {librarianInfo}"
            });

            db.SaveChanges();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
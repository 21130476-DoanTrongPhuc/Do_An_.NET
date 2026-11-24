using LibraryManagement.Data;
using LibraryManagement.Helpers;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services;

public class BookSearchItemDto
{
    public int BookId { get; set; }
    public string BookCode { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string PublishedYear { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int MaxLoanDays { get; set; } = 14;
    public int MaxRenewals { get; set; } = 1;
    public string Description { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public bool IsFavorite { get; set; }
}

public class ActiveLoanItemDto
{
    public int LoanDetailId { get; set; }
    public int BookId { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public string CopyCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime BorrowedAt { get; set; }
    public DateTime DueAt { get; set; }
    public int RenewalCount { get; set; }
    public int MaxRenewals { get; set; }
    public int MaxLoanDays { get; set; }
    public int DaysRemaining => (DueAt.Date - DateTime.Today).Days;
    public bool IsOverdue => DaysRemaining < 0;
    public int OverdueDays => IsOverdue ? Math.Abs(DaysRemaining) : 0;
    public int EstimatedFineVnd => OverdueDays * ReaderPortalService.LateFeePerDayVnd;
}

public class HistoryLoanItemDto
{
    public int LoanDetailId { get; set; }
    public int BookId { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public string CopyCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime BorrowedAt { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string ConditionBefore { get; set; } = string.Empty;
    public string ConditionAfter { get; set; } = string.Empty;
    public int FineAmountVnd { get; set; }
    public int PaidAmountVnd { get; set; }
    public string StatusText { get; set; } = string.Empty;
}

public class ReaderPortalService
{
    public const int LateFeePerDayVnd = 5000;
    public const int DefaultMaxBorrowCopies = 3;
    public const int DefaultLoanDays = 14;

    public int ResolveReaderId()
    {
        if (Session.CurrentReaderId > 0) return Session.CurrentReaderId;
        using var db = new LibraryDbContext();
        var firstReader = db.Users.FirstOrDefault(u => u.UserType == "Reader");
        if (firstReader != null) Session.CurrentReaderId = firstReader.Id;
        return Session.CurrentReaderId;
    }

    public Users? GetCurrentReader()
    {
        int readerId = ResolveReaderId();
        using var db = new LibraryDbContext();
        return db.Users
            .Include(u => u.ReaderCards)
            .FirstOrDefault(u => u.Id == readerId);
    }

    public ReaderCards? GetLatestReaderCard(int readerId)
    {
        using var db = new LibraryDbContext();
        return db.ReaderCards
            .Where(c => c.IdReader == readerId)
            .OrderByDescending(c => c.Status == "Active")
            .ThenByDescending(c => c.ExpiresAt)
            .FirstOrDefault();
    }

    public List<Categories> GetAllCategories()
    {
        using var db = new LibraryDbContext();
        return db.BookCategories
            .Where(c => c.Status == "Active")
            .OrderBy(c => c.IdParentCategory)
            .ThenBy(c => c.CategoryName)
            .ToList();
    }

    public List<BookSearchItemDto> SearchBooks(string keyword = "", int? categoryId = null, bool onlyAvailable = false)
    {
        int readerId = ResolveReaderId();
        using var db = new LibraryDbContext();

        var favBookIds = db.FavoriteBooks
            .Where(f => f.ReaderId == readerId)
            .Select(f => f.BookId)
            .ToHashSet();

        // Lấy danh sách Id thể loại con nếu chọn thể loại cha trên TreeView
        HashSet<int>? categoryFilter = null;
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            var allCats = db.BookCategories.ToList();
            categoryFilter = CollectCategoryIds(allCats, categoryId.Value);
        }

        var query = db.Books
            .Include(b => b.Category)
            .Include(b => b.Copies)
            .AsQueryable();

        if (categoryFilter != null)
            query = query.Where(b => categoryFilter.Contains(b.IdCategory));

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string kw = keyword.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(kw) ||
                b.Author.ToLower().Contains(kw) ||
                b.BookCode.ToLower().Contains(kw) ||
                (b.ISBN != null && b.ISBN.ToLower().Contains(kw)));
        }

        var list = query.OrderBy(b => b.Title).ToList().Select(b =>
        {
            int total = b.Copies.Count;
            int avail = b.Copies.Count(c => c.CirculationStatus == "Available");
            return new BookSearchItemDto
            {
                BookId = b.Id,
                BookCode = b.BookCode,
                Isbn = b.ISBN ?? "N/A",
                Title = b.Title,
                Author = b.Author,
                Publisher = b.Publisher ?? "Chưa cập nhật",
                PublishedYear = b.PublishedYear ?? "N/A",
                CategoryId = b.IdCategory,
                CategoryName = b.Category?.CategoryName ?? "Chưa phân loại",
                MaxLoanDays = b.Category?.MaxLoanDays ?? DefaultLoanDays,
                MaxRenewals = b.Category?.MaxRenewals ?? 1,
                Description = b.Description ?? "Chưa có mô tả cho đầu sách này.",
                TotalCopies = total,
                AvailableCopies = avail,
                IsFavorite = favBookIds.Contains(b.Id)
            };
        });

        if (onlyAvailable)
            list = list.Where(x => x.AvailableCopies > 0);

        return list.ToList();
    }

    private static HashSet<int> CollectCategoryIds(List<Categories> all, int rootId)
    {
        var result = new HashSet<int> { rootId };
        bool added;
        do
        {
            added = false;
            foreach (var c in all)
            {
                if (c.IdParentCategory.HasValue && result.Contains(c.IdParentCategory.Value) && result.Add(c.Id))
                    added = true;
            }
        } while (added);
        return result;
    }

    public (Book? Book, List<BookCopies> Copies, bool IsFavorite) GetBookDetail(int bookId)
    {
        int readerId = ResolveReaderId();
        using var db = new LibraryDbContext();
        var book = db.Books
            .Include(b => b.Category)
            .Include(b => b.Copies)
            .FirstOrDefault(b => b.Id == bookId);

        if (book == null) return (null, new List<BookCopies>(), false);

        bool isFav = db.FavoriteBooks.Any(f => f.ReaderId == readerId && f.BookId == bookId);
        var copies = book.Copies.OrderBy(c => c.CopyCode).ToList();
        return (book, copies, isFav);
    }

    public bool ToggleFavorite(int bookId)
    {
        int readerId = ResolveReaderId();
        using var db = new LibraryDbContext();
        var existing = db.FavoriteBooks.FirstOrDefault(f => f.ReaderId == readerId && f.BookId == bookId);
        if (existing != null)
        {
            db.FavoriteBooks.Remove(existing);
            db.SaveChanges();
            return false;
        }

        db.FavoriteBooks.Add(new FavoriteBook
        {
            ReaderId = readerId,
            BookId = bookId,
            AddedDate = DateTime.Now
        });
        db.SaveChanges();
        return true;
    }

    public List<BookSearchItemDto> GetFavoriteBooks(string keyword = "")
    {
        int readerId = ResolveReaderId();
        using var db = new LibraryDbContext();
        var favIds = db.FavoriteBooks
            .Where(f => f.ReaderId == readerId)
            .OrderByDescending(f => f.AddedDate)
            .Select(f => f.BookId)
            .ToList();

        return SearchBooks(keyword)
            .Where(b => favIds.Contains(b.BookId))
            .ToList();
    }

    public int GetUnpaidFineTotal(int readerId)
    {
        using var db = new LibraryDbContext();
        var fines = db.Fines
            .Include(f => f.LoanDetail)
                .ThenInclude(d => d.Loan)
            .Include(f => f.FinePayments)
            .Where(f => f.LoanDetail.Loan.IdReader == readerId)
            .ToList();

        int debt = 0;
        foreach (var f in fines)
        {
            int paid = f.FinePayments?.Sum(p => p.AmountVnd) ?? 0;
            if (f.AmountVnd > paid)
                debt += (f.AmountVnd - paid);
        }
        return debt;
    }

    /// <summary>
    /// Kiểm tra điều kiện mượn/đặt sách theo nghiệp vụ ảnh nền xanh:
    /// 1. Thẻ độc giả có bị khóa/hết hạn không?
    /// 2. Có đang nợ tiền phạt chưa trả không?
    /// 3. Có đang mượn quá số lượng quy định (tối đa 3 cuốn) không?
    /// </summary>
    public (bool CanBorrow, string Reason) CheckBorrowEligibility(int readerId, bool checkMaxQuota = true)
    {
        using var db = new LibraryDbContext();
        var reader = db.Users.FirstOrDefault(u => u.Id == readerId);
        if (reader == null)
            return (false, "Không tìm thấy thông tin độc giả.");

        if (!reader.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            return (false, "Tài khoản độc giả đang bị vô hiệu hóa.");

        var card = db.ReaderCards
            .Where(c => c.IdReader == readerId)
            .OrderByDescending(c => c.Status == "Active")
            .ThenByDescending(c => c.ExpiresAt)
            .FirstOrDefault();

        if (card == null)
            return (false, "Độc giả chưa được cấp thẻ thư viện.");

        if (card.Status.Equals("Locked", StringComparison.OrdinalIgnoreCase))
            return (false, $"Thẻ thư viện ({card.CardCode}) đang bị KHÓA. Vui lòng liên hệ thủ thư.");

        if (card.Status.Equals("Expired", StringComparison.OrdinalIgnoreCase) || card.ExpiresAt.Date < DateTime.Today)
            return (false, $"Thẻ thư viện ({card.CardCode}) đã HẾT HẠN ngày {card.ExpiresAt:dd/MM/yyyy}.");

        int unpaidFines = GetUnpaidFineTotal(readerId);
        if (unpaidFines > 0)
            return (false, $"Độc giả đang nợ tiền phạt chưa thanh toán: {unpaidFines:N0} VNĐ.");

        int activeBorrowCount = db.LoanDetails
            .Include(d => d.Loan)
            .Count(d => d.Loan.IdReader == readerId && d.ReturnedAt == null);

        int maxAllowed = reader.MaxActiveCopies ?? DefaultMaxBorrowCopies;
        if (checkMaxQuota && activeBorrowCount >= maxAllowed)
            return (false, $"Độc giả đã mượn đủ số lượng tối đa cho phép ({activeBorrowCount}/{maxAllowed} cuốn).");

        return (true, "Đủ điều kiện mượn sách.");
    }

    public (bool Success, string Message) CreateReservation(int bookId)
    {
        int readerId = ResolveReaderId();
        var check = CheckBorrowEligibility(readerId, checkMaxQuota: true);
        if (!check.CanBorrow)
            return (false, check.Reason);

        using var db = new LibraryDbContext();
        bool alreadyReserved = db.Reservations.Any(r =>
            r.IdReader == readerId &&
            r.IdBook == bookId &&
            (r.Status == "Pending" || r.Status == "ReadyForPickup"));

        if (alreadyReserved)
            return (false, "Bạn đã đặt trước đầu sách này và đang chờ xử lý.");

        bool alreadyBorrowing = db.LoanDetails
            .Include(d => d.Loan)
            .Include(d => d.BookCopy)
            .Any(d => d.Loan.IdReader == readerId && d.ReturnedAt == null && d.BookCopy.IdBook == bookId);

        if (alreadyBorrowing)
            return (false, "Bạn đang mượn một bản sao của đầu sách này.");

        int nextId = (db.Reservations.Any() ? db.Reservations.Max(r => r.Id) : 0) + 1;
        var res = new Reservations
        {
            ReservationCode = $"RS{nextId:D4}",
            IdReader = readerId,
            IdBook = bookId,
            ReservedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddDays(3),
            Status = "Pending",
            Note = "Độc giả đặt trước qua Cổng độc giả"
        };

        db.Reservations.Add(res);
        db.SaveChanges();
        return (true, $"Đặt trước thành công! Mã phiếu đặt: {res.ReservationCode} (hiệu lực giữ chỗ đến {res.ExpiresAt:dd/MM/yyyy}).");
    }

    public List<ActiveLoanItemDto> GetActiveLoans()
    {
        int readerId = ResolveReaderId();
        using var db = new LibraryDbContext();

        return db.LoanDetails
            .Include(d => d.Loan)
            .Include(d => d.BookCopy)
                .ThenInclude(c => c.Book)
                    .ThenInclude(b => b.Category)
            .Where(d => d.Loan.IdReader == readerId && d.ReturnedAt == null)
            .OrderBy(d => d.DueAt)
            .ToList()
            .Select(d => new ActiveLoanItemDto
            {
                LoanDetailId = d.Id,
                BookId = d.BookCopy.IdBook,
                LoanCode = d.Loan.LoanCode,
                CopyCode = d.BookCopy.CopyCode,
                Title = d.BookCopy.Book.Title,
                Author = d.BookCopy.Book.Author,
                BorrowedAt = d.Loan.BorrowedAt,
                DueAt = d.DueAt,
                RenewalCount = d.RenewalCount,
                MaxRenewals = d.BookCopy.Book.Category?.MaxRenewals ?? 1,
                MaxLoanDays = d.BookCopy.Book.Category?.MaxLoanDays ?? DefaultLoanDays
            })
            .ToList();
    }

    public (bool Success, string Message) RenewLoan(int loanDetailId)
    {
        int readerId = ResolveReaderId();
        var check = CheckBorrowEligibility(readerId, checkMaxQuota: false);
        if (!check.CanBorrow)
            return (false, $"Không thể gia hạn: {check.Reason}");

        using var db = new LibraryDbContext();
        var detail = db.LoanDetails
            .Include(d => d.Loan)
            .Include(d => d.BookCopy)
                .ThenInclude(c => c.Book)
                    .ThenInclude(b => b.Category)
            .FirstOrDefault(d => d.Id == loanDetailId && d.Loan.IdReader == readerId);

        if (detail == null || detail.ReturnedAt != null)
            return (false, "Không tìm thấy lượt mượn hợp lệ.");

        if (detail.DueAt.Date < DateTime.Today)
            return (false, "Sách đã quá hạn trả, không thể tự gia hạn. Vui lòng mang sách đến quầy thủ thư.");

        int maxRenewals = detail.BookCopy.Book.Category?.MaxRenewals ?? 1;
        if (detail.RenewalCount >= maxRenewals)
            return (false, $"Cuốn sách này đã đạt giới hạn gia hạn tối đa ({maxRenewals} lần).");

        bool hasPendingReservation = db.Reservations.Any(r =>
            r.IdBook == detail.BookCopy.IdBook &&
            r.IdReader != readerId &&
            (r.Status == "Pending" || r.Status == "ReadyForPickup"));

        if (hasPendingReservation)
            return (false, "Không thể gia hạn vì đang có độc giả khác đặt trước đầu sách này.");

        int addDays = detail.BookCopy.Book.Category?.MaxLoanDays ?? DefaultLoanDays;
        detail.DueAt = detail.DueAt.AddDays(addDays);
        detail.RenewalCount += 1;
        db.SaveChanges();

        return (true, $"Gia hạn thành công thêm {addDays} ngày! Hạn trả mới: {detail.DueAt:dd/MM/yyyy}.");
    }

    public List<HistoryLoanItemDto> GetBorrowHistory(string statusFilter = "Tất cả", string keyword = "")
    {
        int readerId = ResolveReaderId();
        using var db = new LibraryDbContext();

        var details = db.LoanDetails
            .Include(d => d.Loan)
            .Include(d => d.BookCopy)
                .ThenInclude(c => c.Book)
            .Include(d => d.Fines)
                .ThenInclude(f => f.FinePayments)
            .Where(d => d.Loan.IdReader == readerId)
            .OrderByDescending(d => d.Loan.BorrowedAt)
            .ToList();

        var rows = details.Select(d =>
        {
            int dbFine = d.Fines?.Sum(f => f.AmountVnd) ?? 0;
            int paid = d.Fines?.SelectMany(f => f.FinePayments ?? Enumerable.Empty<FinePayments>()).Sum(p => p.AmountVnd) ?? 0;

            // Nếu chưa trả và quá hạn, tính phạt tạm tính 5.000đ/ngày theo quy định
            int overdueDays = d.ReturnedAt == null && d.DueAt.Date < DateTime.Today
                ? (DateTime.Today - d.DueAt.Date).Days
                : 0;
            int totalFine = dbFine > 0 ? dbFine : overdueDays * LateFeePerDayVnd;

            string status = d.ReturnedAt != null
                ? "Đã trả"
                : (d.DueAt.Date < DateTime.Today ? "Quá hạn" : "Đang mượn");

            return new HistoryLoanItemDto
            {
                LoanDetailId = d.Id,
                BookId = d.BookCopy.IdBook,
                LoanCode = d.Loan.LoanCode,
                CopyCode = d.BookCopy.CopyCode,
                Title = d.BookCopy.Book.Title,
                Author = d.BookCopy.Book.Author,
                BorrowedAt = d.Loan.BorrowedAt,
                DueAt = d.DueAt,
                ReturnedAt = d.ReturnedAt,
                ConditionBefore = d.PhysicalConditionBefore,
                ConditionAfter = d.PhysicalConditionAfter ?? "-",
                FineAmountVnd = totalFine,
                PaidAmountVnd = paid,
                StatusText = status
            };
        });

        if (statusFilter != "Tất cả")
            rows = rows.Where(r => r.StatusText == statusFilter);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string kw = keyword.Trim().ToLower();
            rows = rows.Where(r =>
                r.Title.ToLower().Contains(kw) ||
                r.LoanCode.ToLower().Contains(kw) ||
                r.CopyCode.ToLower().Contains(kw));
        }

        return rows.ToList();
    }

    public (bool Success, string Message) UpdateProfile(string fullName, string phone, string email, string address, string dob)
    {
        int readerId = ResolveReaderId();
        using var db = new LibraryDbContext();
        var user = db.Users.FirstOrDefault(u => u.Id == readerId);
        if (user == null) return (false, "Không tìm thấy tài khoản.");

        if (!string.IsNullOrWhiteSpace(email) &&
            db.Users.Any(u => u.Id != readerId && u.Email == email))
            return (false, "Email này đã được sử dụng bởi tài khoản khác.");

        user.FullName = fullName.Trim();
        user.Phone = phone.Trim();
        user.Email = email.Trim();
        user.Address = address.Trim();
        user.DateOfBirth = dob.Trim();
        user.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        db.SaveChanges();
        return (true, "Cập nhật thông tin cá nhân thành công!");
    }

    public (bool Success, string Message) ChangePassword(string oldPass, string newPass)
    {
        int readerId = ResolveReaderId();
        using var db = new LibraryDbContext();
        var user = db.Users.FirstOrDefault(u => u.Id == readerId);
        if (user == null) return (false, "Không tìm thấy tài khoản.");

        if (user.PasswordHash != oldPass)
            return (false, "Mật khẩu hiện tại không chính xác!");

        user.PasswordHash = newPass;
        user.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        db.SaveChanges();
        return (true, "Đổi mật khẩu thành công!");
    }
}
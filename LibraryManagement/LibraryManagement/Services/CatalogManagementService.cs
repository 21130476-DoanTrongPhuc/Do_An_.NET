using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services;

public class CatalogManagementService
{
    // ==================== 1.4. THỂ LOẠI & DANH MỤC SÁCH ====================

    public List<Categories> GetAllCategories()
    {
        using var db = new LibraryDbContext();
        return db.BookCategories
            .Include(c => c.Books)
            .Include(c => c.Children)
            .OrderBy(c => c.CategoryCode)
            .ToList();
    }

    public int CountBooksInCategoryTree(int categoryId, List<Categories> allCategories)
    {
        var target = allCategories.FirstOrDefault(c => c.Id == categoryId);
        if (target == null) return 0;

        int directCount = target.Books.Count;
        var children = allCategories.Where(c => c.IdParentCategory == categoryId).ToList();
        return directCount + children.Sum(ch => CountBooksInCategoryTree(ch.Id, allCategories));
    }

    public void SaveCategory(Categories category)
    {
        if (string.IsNullOrWhiteSpace(category.CategoryCode) || string.IsNullOrWhiteSpace(category.CategoryName))
            throw new InvalidOperationException("Mã thể loại và tên thể loại không được để trống.");

        if (category.Id > 0 && category.IdParentCategory == category.Id)
            throw new InvalidOperationException("Thể loại cha không thể là chính nó.");

        using var db = new LibraryDbContext();
        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        bool duplicateCode = db.BookCategories.Any(c =>
            c.CategoryCode.ToLower() == category.CategoryCode.Trim().ToLower() && c.Id != category.Id);
        if (duplicateCode)
            throw new InvalidOperationException($"Mã thể loại '{category.CategoryCode}' đã tồn tại.");

        if (category.Id == 0)
        {
            category.CategoryCode = category.CategoryCode.Trim().ToUpper();
            category.CategoryName = category.CategoryName.Trim();
            category.Status = string.IsNullOrWhiteSpace(category.Status) ? "Active" : category.Status;
            category.CreatedAt = now;
            category.UpdatedAt = now;
            db.BookCategories.Add(category);
        }
        else
        {
            var existing = db.BookCategories.FirstOrDefault(c => c.Id == category.Id)
                ?? throw new InvalidOperationException("Không tìm thấy thể loại cần cập nhật.");

            existing.IdParentCategory = category.IdParentCategory;
            existing.CategoryCode = category.CategoryCode.Trim().ToUpper();
            existing.CategoryName = category.CategoryName.Trim();
            existing.MaxLoanDays = category.MaxLoanDays;
            existing.MaxRenewals = category.MaxRenewals;
            existing.Status = category.Status;
            existing.UpdatedAt = now;
        }

        db.SaveChanges();
    }

    public void DeleteCategory(int categoryId)
    {
        using var db = new LibraryDbContext();
        var cat = db.BookCategories
            .Include(c => c.Books)
            .Include(c => c.Children)
            .FirstOrDefault(c => c.Id == categoryId)
            ?? throw new InvalidOperationException("Không tìm thấy thể loại.");

        if (cat.Books.Any())
            throw new InvalidOperationException($"Không thể xóa thể loại '{cat.CategoryName}' vì đang có {cat.Books.Count} đầu sách sử dụng.");

        if (cat.Children.Any())
            throw new InvalidOperationException($"Không thể xóa thể loại '{cat.CategoryName}' vì đang chứa {cat.Children.Count} thể loại con.");

        db.BookCategories.Remove(cat);
        db.SaveChanges();
    }

    // ==================== 1.1. QUẢN LÝ ĐẦU SÁCH (BOOKS) ====================

    public List<Book> SearchBooks(string? keyword, int? categoryId, bool includeDiscontinued = true)
    {
        using var db = new LibraryDbContext();
        var query = db.Books
            .Include(b => b.Category)
            .Include(b => b.Copies)
            .AsQueryable();

        if (!includeDiscontinued)
            query = query.Where(b => b.Status == "Active");

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            var allCats = db.BookCategories.ToList();
            var validIds = GetDescendantCategoryIds(categoryId.Value, allCats);
            query = query.Where(b => validIds.Contains(b.IdCategory));
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string kw = keyword.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(kw) ||
                b.Author.ToLower().Contains(kw) ||
                b.BookCode.ToLower().Contains(kw) ||
                (b.ISBN != null && b.ISBN.ToLower().Contains(kw)) ||
                b.Category.CategoryName.ToLower().Contains(kw));
        }

        return query.OrderByDescending(b => b.Id).ToList();
    }

    private static HashSet<int> GetDescendantCategoryIds(int rootId, List<Categories> all)
    {
        var result = new HashSet<int> { rootId };
        var queue = new Queue<int>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            foreach (var child in all.Where(c => c.IdParentCategory == current))
            {
                if (result.Add(child.Id))
                    queue.Enqueue(child.Id);
            }
        }
        return result;
    }

    public string GenerateNextBookCode()
    {
        using var db = new LibraryDbContext();
        int nextId = (db.Books.Max(b => (int?)b.Id) ?? 0) + 1;
        return $"BK{nextId:D4}";
    }

    public void SaveBook(Book book)
    {
        if (string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.Author))
            throw new InvalidOperationException("Tên sách và tác giả là bắt buộc.");
        if (book.IdCategory <= 0)
            throw new InvalidOperationException("Vui lòng chọn thể loại cho đầu sách.");

        using var db = new LibraryDbContext();
        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        if (!string.IsNullOrWhiteSpace(book.ISBN))
        {
            bool dupIsbn = db.Books.Any(b => b.ISBN == book.ISBN.Trim() && b.Id != book.Id);
            if (dupIsbn)
                throw new InvalidOperationException($"Mã ISBN '{book.ISBN}' đã tồn tại ở đầu sách khác.");
        }

        if (book.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(book.BookCode))
                book.BookCode = GenerateNextBookCode();

            book.Status = "Active";
            book.CreatedAt = now;
            book.UpdatedAt = now;
            db.Books.Add(book);
        }
        else
        {
            var existing = db.Books.FirstOrDefault(b => b.Id == book.Id)
                ?? throw new InvalidOperationException("Không tìm thấy đầu sách.");

            existing.IdCategory = book.IdCategory;
            existing.BookCode = book.BookCode.Trim();
            existing.ISBN = string.IsNullOrWhiteSpace(book.ISBN) ? null : book.ISBN.Trim();
            existing.Title = book.Title.Trim();
            existing.Author = book.Author.Trim();
            existing.Publisher = book.Publisher?.Trim();
            existing.PublishedYear = book.PublishedYear?.Trim();
            existing.Language = book.Language?.Trim();
            existing.PageCount = book.PageCount;
            existing.CoverImagePath = book.CoverImagePath;
            existing.Status = book.Status;
            existing.Description = book.Description;
            existing.UpdatedAt = now;
        }

        db.SaveChanges();
    }

    public void ToggleBookDiscontinued(int bookId)
    {
        using var db = new LibraryDbContext();
        var book = db.Books.Include(b => b.Copies).FirstOrDefault(b => b.Id == bookId)
            ?? throw new InvalidOperationException("Không tìm thấy đầu sách.");

        if (book.Status == "Active")
        {
            bool hasBorrowed = book.Copies.Any(c => c.CirculationStatus == "Borrowed");
            if (hasBorrowed)
                throw new InvalidOperationException("Đầu sách vẫn còn bản sách đang được mượn, không thể ngừng lưu hành.");

            book.Status = "Discontinued";
        }
        else
        {
            book.Status = "Active";
        }

        book.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        db.SaveChanges();
    }

    // ==================== 1.2. QUẢN LÝ BẢN SÁCH VẬT LÝ (BOOKCOPIES) ====================

    public List<BookCopies> SearchBookCopies(string? keyword, int? bookId = null)
    {
        using var db = new LibraryDbContext();
        var query = db.BookCopies
            .Include(c => c.Book)
            .Include(c => c.LoanDetails)
            .AsQueryable();

        if (bookId.HasValue && bookId.Value > 0)
            query = query.Where(c => c.IdBook == bookId.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string kw = keyword.Trim().ToLower();
            query = query.Where(c =>
                c.CopyCode.ToLower().Contains(kw) ||
                c.Book.Title.ToLower().Contains(kw) ||
                c.Book.BookCode.ToLower().Contains(kw) ||
                (c.ShelfLocation != null && c.ShelfLocation.ToLower().Contains(kw)));
        }

        return query.OrderByDescending(c => c.Id).ToList();
    }

    public string GenerateNextCopyCode(int bookId)
    {
        using var db = new LibraryDbContext();
        var book = db.Books.FirstOrDefault(b => b.Id == bookId);
        string prefix = book?.BookCode ?? "CP";
        int count = db.BookCopies.Count(c => c.IdBook == bookId) + 1;
        string candidate = $"{prefix}-{count:D3}";
        while (db.BookCopies.Any(c => c.CopyCode == candidate))
        {
            count++;
            candidate = $"{prefix}-{count:D3}";
        }
        return candidate;
    }

    public void SaveBookCopy(BookCopies copy, string? changeNote = null)
    {
        if (copy.IdBook <= 0)
            throw new InvalidOperationException("Vui lòng chọn đầu sách.");
        if (string.IsNullOrWhiteSpace(copy.CopyCode))
            throw new InvalidOperationException("Mã bản sách / mã vạch không được để trống.");

        using var db = new LibraryDbContext();
        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        bool dupCode = db.BookCopies.Any(c =>
            c.CopyCode.ToLower() == copy.CopyCode.Trim().ToLower() && c.Id != copy.Id);
        if (dupCode)
            throw new InvalidOperationException($"Mã bản sách '{copy.CopyCode}' đã tồn tại.");

        if (copy.Id == 0)
        {
            copy.CopyCode = copy.CopyCode.Trim().ToUpper();
            copy.CreatedAt = DateTime.Now;
            copy.UpdatedAt = DateTime.Now;
            db.BookCopies.Add(copy);
            db.SaveChanges();

            db.BookCopyHistories.Add(new BookCopyHistory
            {
                IdBookCopy = copy.Id,
                ChangedAt = now,
                ActionType = "Nhập mới bản sách",
                NewStatus = copy.CirculationStatus,
                NewCondition = copy.PhysicalCondition,
                NewLocation = copy.ShelfLocation,
                Note = changeNote ?? $"Nguồn nhập: {copy.Source ?? "Không rõ"}"
            });
            db.SaveChanges();
        }
        else
        {
            var existing = db.BookCopies.FirstOrDefault(c => c.Id == copy.Id)
                ?? throw new InvalidOperationException("Không tìm thấy bản sách.");

            string oldStatus = existing.CirculationStatus;
            string oldCond = existing.PhysicalCondition;
            string? oldLoc = existing.ShelfLocation;

            existing.CopyCode = copy.CopyCode.Trim().ToUpper();
            existing.ShelfLocation = copy.ShelfLocation?.Trim();
            existing.ReceivedAt = copy.ReceivedAt;
            existing.PurchasePrice = copy.PurchasePrice;
            existing.Source = copy.Source?.Trim();
            existing.PhysicalCondition = copy.PhysicalCondition;
            existing.CirculationStatus = copy.CirculationStatus;
            existing.Notes = copy.Notes;
            existing.UpdatedAt = DateTime.Now;

            if (oldStatus != existing.CirculationStatus || oldCond != existing.PhysicalCondition || oldLoc != existing.ShelfLocation)
            {
                db.BookCopyHistories.Add(new BookCopyHistory
                {
                    IdBookCopy = existing.Id,
                    ChangedAt = now,
                    ActionType = "Cập nhật thông tin / trạng thái",
                    OldStatus = oldStatus,
                    NewStatus = existing.CirculationStatus,
                    OldCondition = oldCond,
                    NewCondition = existing.PhysicalCondition,
                    OldLocation = oldLoc,
                    NewLocation = existing.ShelfLocation,
                    Note = changeNote ?? existing.Notes
                });
            }
            db.SaveChanges();
        }
    }

    public void ChangeCopyStatus(int copyId, string newStatus, string? newCondition = null, string? reason = null)
    {
        using var db = new LibraryDbContext();
        var copy = db.BookCopies.FirstOrDefault(c => c.Id == copyId)
            ?? throw new InvalidOperationException("Không tìm thấy bản sách.");

        if (copy.CirculationStatus == "Borrowed")
            throw new InvalidOperationException("Bản sách đang được độc giả mượn, không thể đổi trạng thái trực tiếp.");

        string oldStatus = copy.CirculationStatus;
        string oldCond = copy.PhysicalCondition;

        copy.CirculationStatus = newStatus;
        if (!string.IsNullOrWhiteSpace(newCondition))
            copy.PhysicalCondition = newCondition;
        copy.UpdatedAt = DateTime.Now;

        db.BookCopyHistories.Add(new BookCopyHistory
        {
            IdBookCopy = copy.Id,
            ChangedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ActionType = $"Chuyển trạng thái -> {newStatus}",
            OldStatus = oldStatus,
            NewStatus = newStatus,
            OldCondition = oldCond,
            NewCondition = copy.PhysicalCondition,
            Note = reason
        });

        db.SaveChanges();
    }

    public void DeleteBookCopy(int copyId)
    {
        using var db = new LibraryDbContext();
        var copy = db.BookCopies
            .Include(c => c.LoanDetails)
            .Include(c => c.Reservations)
            .FirstOrDefault(c => c.Id == copyId)
            ?? throw new InvalidOperationException("Không tìm thấy bản sách.");

        // Quy tắc nghiệp vụ 1.2: Không cho phép xóa bản sách đã phát sinh giao dịch mượn/trả
        if (copy.LoanDetails.Any() || copy.Reservations.Any())
            throw new InvalidOperationException(
                "Bản sách này đã phát sinh lịch sử giao dịch mượn/trả hoặc đặt trước nên KHÔNG ĐƯỢC XÓA.\n" +
                "Vui lòng chuyển sang trạng thái 'Thanh lý' (Disposed) hoặc 'Bảo trì'.");

        db.BookCopies.Remove(copy);
        db.SaveChanges();
    }

    public List<BookCopyHistory> GetCopyHistories(int copyId)
    {
        using var db = new LibraryDbContext();
        return db.BookCopyHistories
            .Where(h => h.IdBookCopy == copyId)
            .OrderByDescending(h => h.Id)
            .ToList();
    }
}
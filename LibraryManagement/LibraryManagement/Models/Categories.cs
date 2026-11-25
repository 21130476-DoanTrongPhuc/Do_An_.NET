using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class Categories
{
    public int Id { get; set; }
    public int? IdParent { get; set; }              // ← đổi tên khớp cột "IdParent"
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int MaxLoanDays { get; set; }
    public int MaxRenewals { get; set; }
    public BookCategoryStatus Status { get; set; } = BookCategoryStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Categories? ParentCategory { get; set; }
    public ICollection<Categories> Children { get; set; } = new List<Categories>();
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
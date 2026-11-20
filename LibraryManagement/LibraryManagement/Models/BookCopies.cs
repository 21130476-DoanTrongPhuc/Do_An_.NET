using LibraryManagement.Enums;

namespace LibraryManagement.Models;

// Trong file Models/BookCopies.cs:
public class BookCopies
{
    public int Id { get; set; }
    public int IdBook { get; set; }
    public string CopyCode { get; set; } = string.Empty;
    public string? ShelfLocation { get; set; }
    public DateTime ReceivedAt { get; set; }
    public int PurchasePriceVnd { get; set; }
    public string PhysicalCondition { get; set; } = "Good";
    public BookCopyStatus CirculationStatus { get; set; }

    // Sửa từ Notes -> Note (số ít) để khớp với cột "Note" trong SQLite
    public string? Note { get; set; } 

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Book Book { get; set; } = null!;
    public ICollection<LoanDetails> LoanDetails { get; set; } = new List<LoanDetails>();
}
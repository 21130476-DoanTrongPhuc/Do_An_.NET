using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class BookCopies
{
    public int Id { get; set; }
    public int IdBook { get; set; }
    public string CopyCode { get; set; } = string.Empty;
    public string? ShelfLocation { get; set; }
    public DateTime ReceivedAt { get; set; }
    public int PurchasePriceVnd { get; set; }
    public BookCondition PhysicalCondition { get; set; } = BookCondition.Good;   // ← enum thay vì string
    public BookCopyStatus CirculationStatus { get; set; } = BookCopyStatus.Available;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Book Book { get; set; } = null!;
    public ICollection<LoanDetails> LoanDetails { get; set; } = new List<LoanDetails>();
}
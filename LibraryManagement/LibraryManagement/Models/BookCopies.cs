using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class BookCopies
{
    public int Id { get; set; }
    public int IdBook { get; set; }
    public string CopyCode { get; set; } = string.Empty;
    public string? ShelfLocation { get; set; }
    public string ReceivedAt { get; set; }
    public int PurchasePriceVnd { get; set; }
    public BookCondition PhysicalCondition { get; set; } = BookCondition.Good;   // ← enum thay vì string
    public CirculationStatus CirculationStatus { get; set; } = CirculationStatus.Available;
    public string? Notes { get; set; }
    public string? Source { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation

    public Book Book { get; set; } = null!;
    public ICollection<LoanDetails> LoanDetails { get; set; } = new List<LoanDetails>();
    public ICollection<Reservations> Reservations { get; set; } = new List<Reservations>();
}

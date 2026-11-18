namespace LibraryManagement.Models;

public class BookCopies
{
    public int Id { get; set; }
    public int IdBook { get; set; }
    public string CopyCode { get; set; } = string.Empty;
    public string? ShelfLocation { get; set; }
    public string ReceivedAt { get; set; } = string.Empty;
    public int PurchasePrice { get; set; }
    public string? Source { get; set; }
    public string PhysicalCondition { get; set; } = "Good"; // New, Good, Worn, Damaged
    public string CirculationStatus { get; set; } = "Available"; // Available, Borrowed, Reserved, Maintenance, Lost, Disposed
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Book Book { get; set; } = null!;
    public ICollection<LoanDetails> LoanDetails { get; set; } = new List<LoanDetails>();
    public ICollection<Reservations> Reservations { get; set; } = new List<Reservations>();
}
public class BookCopyHistory
{
    public int Id { get; set; }
    public int IdBookCopy { get; set; }
    public string ChangedAt { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? OldStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? OldCondition { get; set; }
    public string? NewCondition { get; set; }
    public string? OldLocation { get; set; }
    public string? NewLocation { get; set; }
    public string? Note { get; set; }
}
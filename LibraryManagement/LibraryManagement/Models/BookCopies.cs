namespace LibraryManagement.Models;

public class BookCopies
{
    public int Id { get; set; }
    public int IdBook { get; set; }
    public string CopyCode { get; set; } = string.Empty;
    public string? ShelfLocation { get; set; }
    public string ReceivedAt  { get; set; } = string.Empty;
    public int PurchasePrice { get; set; }
    public string PhysicalCondition { get; set; } = string.Empty;
    public string CirculationStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public Book Book { get; set; } = null;
    public ICollection<LoanDetails> LoanDetails { get; set; } = new List<LoanDetails>();
    public ICollection<Reservations>  Reservations { get; set; } = new List<Reservations>();
}
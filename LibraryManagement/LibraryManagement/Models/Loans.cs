namespace LibraryManagement.Models;

public class Loans
{
    public int Id { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public int IdReader { get; set; }
    public int IdLibrarian { get; set; }
    public string BorrowedAt { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public Readers Reader { get; set; } = null;
    public Librarians Librarian { get; set; } = null;
    public ICollection<LoanDetails> Details { get; set; } = new List<LoanDetails>();
    public ICollection<Reservations> Reservations { get; set; } = new List<Reservations>();
}
namespace LibraryManagement.Models;

public class Readers
{
    public long Id { get; set; }
    public int MaxActiveCopies { get; set; }
    public string ReaderCode { get; set; } = null;
    public string FullName { get; set; } = string.Empty;
    public string? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    
    public ICollection<ReaderCards> Cards { get; set; } = new List<ReaderCards>();
    public ICollection<Loans> LoanDetails { get; set; } = new List<Loans>();
    public ICollection<Reservations>  Reservations { get; set; } = new List<Reservations>();
}
namespace LibraryManagement.Models;

public class Fines
{
    public int  Id { get; set; }
    public string FineCode { get; set; } = string.Empty;
    public int IdLoanDetails { get; set; } = 0;
    public int IdLibrarian { get; set; } = 0;
    public string CreatedAt { get; set; } = string.Empty;
    public string ViolationType { get; set; } = string.Empty;
    public int Amount { get; set; } 
    public string? Description { get; set; }
    
    public LoanDetails LoanDetails { get; set; } = null;
    public Librarians  Librarian { get; set; } = null;
    public ICollection<FinePayments>  FinePayments { get; set; } = new List<FinePayments>();
}
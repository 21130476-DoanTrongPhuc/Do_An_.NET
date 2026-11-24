namespace LibraryManagement.Models;

public class Loans
{
    public int Id { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public int IdReader { get; set; }
    public DateTime BorrowedAt { get; set; }
    public string? Note { get; set; }               // ← nullable

    // Navigation
    public Users Reader { get; set; } = null!;
    public ICollection<LoanDetails> LoanDetails { get; set; } = new List<LoanDetails>();
}
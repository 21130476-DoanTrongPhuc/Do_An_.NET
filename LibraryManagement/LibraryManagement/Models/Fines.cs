using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class Fines
{
    public int Id { get; set; }
    public string FineCode { get; set; }
    public int IdLoanDetail { get; set; }
    public DateTime CreatedAt { get; set; }
    public ViolationType ViolationType { get; set; }
    public int AmountVnd { get; set; }
    public string Description { get; set; }

    // Navigation
    public LoanDetails LoanDetail { get; set; }
    public ICollection<FinePayments> FinePayments { get; set; }
}
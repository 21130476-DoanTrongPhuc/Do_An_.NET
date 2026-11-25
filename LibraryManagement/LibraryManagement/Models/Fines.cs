using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class Fines
{
    public int Id { get; set; }
    public string FineCode { get; set; } = "";
    public int IdLoanDetail { get; set; }          // ← nullable (SQL cho phép)
    public DateTime CreatedAt { get; set; }
    public ViolationType ViolationType { get; set; }
    public int AmountVnd { get; set; }              // ← int (SQL INTEGER)
    public string? Description { get; set; }        // ← nullable (SQL TEXT)
    public FineStatus Status { get; set; } = FineStatus.Unpaid;
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public LoanDetails? LoanDetail { get; set; }
    public ICollection<FinePayments> FinePayments { get; set; } = new List<FinePayments>();
    public ICollection<FineAuditLogs> AuditLogs { get; set; } = new List<FineAuditLogs>();
}
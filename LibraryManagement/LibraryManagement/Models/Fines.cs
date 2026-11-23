using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class Fines
{
    public int Id { get; set; }
    public string FineCode { get; set; } = "";
    public int? IdLoanDetail { get; set; }          // nullable nếu cho phạt thủ công
    public DateTime CreatedAt { get; set; }
    public ViolationType ViolationType { get; set; }
    public int AmountVnd { get; set; }
    public string Description { get; set; } = "";
    public FineStatus Status { get; set; } = FineStatus.Unpaid;          // THÊM (Unpaid/Partial/Paid/Cancelled)
    public DateTime? CancelledAt { get; set; }       // THÊM
    public string? CancelReason { get; set; }        // THÊM
    public DateTime? UpdatedAt { get; set; }         // THÊM

    // Navigation
    public LoanDetails? LoanDetail { get; set; }
    public ICollection<FinePayments> FinePayments { get; set; } 
    public ICollection<FineAuditLogs> AuditLogs { get; set; } 
}
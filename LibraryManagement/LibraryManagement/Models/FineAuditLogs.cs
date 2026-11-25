using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class FineAuditLogs
{
    public int Id { get; set; }
    public int IdFine { get; set; }
    public FineAuditAction Action { get; set; }
    public int BeforeAmount { get; set; }           
    public int AfterAmount { get; set; }            
    public string? Reason { get; set; }
    public DateTime At { get; set; }

    // Navigation
    public Fines Fine { get; set; } = null!;
}
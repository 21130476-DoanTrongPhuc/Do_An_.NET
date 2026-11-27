using LibraryManagement.Enums;

namespace LibraryManagement.DTO.Response;

public class FineAuditInfo
{
    public DateTime At { get; set; }
    public FineAuditAction Action { get; set; }
    public int BeforeAmount { get; set; }
    public int AfterAmount { get; set; }
    public string? Reason { get; set; }
    public string? ActorName { get; set; }
}
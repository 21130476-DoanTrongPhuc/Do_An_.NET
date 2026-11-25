namespace LibraryManagement.DTO.Response;

public class FineDetailInfo
{
    public int FineId { get; set; }
    public string FineCode { get; set; } = "";
    public List<FinePaymentInfo> Payments { get; set; } = new();
    public List<FineAuditInfo> Audits { get; set; } = new();
}
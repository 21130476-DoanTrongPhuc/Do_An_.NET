namespace LibraryManagement.DTO.Response;

public class ReservationApproveResultResponse
{
    public string ReservationCode { get; set; } = string.Empty;
    public string CopyCode { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string TargetContact { get; set; } = string.Empty;
}
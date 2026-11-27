namespace LibraryManagement.DTO.Request;

public class ReservationApproveRequest
{
    public int ReservationId { get; set; }
    public int BookCopyId { get; set; }
    public int HoldDays { get; set; }
    public string NotifyMessage { get; set; } = string.Empty;
}
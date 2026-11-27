namespace LibraryManagement.DTO.Response;

public class ReservationListSummaryResponse
{
    public int PendingCount { get; set; }
    public int ReadyCount { get; set; }
    public List<ReservationListItemResponse> Items { get; set; } = new();
}
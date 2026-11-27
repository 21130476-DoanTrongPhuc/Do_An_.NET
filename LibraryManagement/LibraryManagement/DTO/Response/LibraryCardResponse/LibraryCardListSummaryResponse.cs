namespace LibraryManagement.DTO.Response;

public class LibraryCardListSummaryResponse
{
    public int ActiveCount { get; set; }
    public int ExpiringCount { get; set; }
    public int ExpiredOrLockedCount { get; set; }
    public int MissingCount { get; set; }
    public List<LibraryCardListItemResponse> Items { get; set; } = new();
}
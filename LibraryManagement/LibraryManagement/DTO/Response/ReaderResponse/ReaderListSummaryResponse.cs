namespace LibraryManagement.DTO.Response;

public class ReaderListSummaryResponse
{
    public int TotalReaders { get; set; }
    public int ActiveReaders { get; set; }
    public List<ReaderListItemResponse> Items { get; set; } = new();
}
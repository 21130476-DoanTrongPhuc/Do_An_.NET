namespace LibraryManagement.DTO.Response;

public class ReaderListItemResponse
{
    public int Id { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string ExpireDateText { get; set; } = string.Empty;
    public string BorrowText { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
}
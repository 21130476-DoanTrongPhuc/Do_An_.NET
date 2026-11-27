namespace LibraryManagement.DTO.Response;

public class LibraryCardListItemResponse
{
    public int ReaderId { get; set; }
    public int? CardId { get; set; }
    public string CardCode { get; set; } = string.Empty;
    public string ReaderDisplay { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string IssuedAtText { get; set; } = string.Empty;
    public string ExpiresAtText { get; set; } = string.Empty;
    public string DisplayStatus { get; set; } = string.Empty;
}
namespace LibraryManagement.DTO.Response;

public class LibraryCardDetailResponse
{
    public int ReaderId { get; set; }
    public string ReaderFullName { get; set; } = string.Empty;
    public string ReaderCode { get; set; } = string.Empty;

    public int? CardId { get; set; }
    public bool HasCard { get; set; }
    public string CardCode { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Note { get; set; } = string.Empty;

    public bool IsLocked { get; set; }
    public bool IsExpired { get; set; }
}
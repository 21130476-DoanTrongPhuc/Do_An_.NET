namespace LibraryManagement.DTO.Request;

public class LibraryCardSaveRequest
{
    public int ReaderId { get; set; }
    public int? CardId { get; set; } 
    public string CardCode { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? Note { get; set; }
}
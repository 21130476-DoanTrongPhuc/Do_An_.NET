namespace LibraryManagement.DTO.Response;

public class ReaderDetailResponse
{
    public int Id { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public bool HasCard { get; set; }
    public string CardCode { get; set; } = string.Empty;
    public DateTime? CardExpiresAt { get; set; }
    public bool IsCardExpired { get; set; }

    public int BorrowingCount { get; set; }
    public int OverdueCount { get; set; }
    public int TotalHistoryCount { get; set; }
}
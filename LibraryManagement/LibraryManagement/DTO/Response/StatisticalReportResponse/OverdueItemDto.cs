namespace LibraryManagement.DTO.Response;

public class OverdueItemDto
{
    public string LoanCode { get; set; } = string.Empty;
    public string ReaderCode { get; set; } = string.Empty;
    public string ReaderName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string CopyCode { get; set; } = string.Empty;
    public DateTime BorrowedAt { get; set; }
    public DateTime DueAt { get; set; }
    public int LateDays { get; set; }
    public decimal EstimatedFine { get; set; }
}
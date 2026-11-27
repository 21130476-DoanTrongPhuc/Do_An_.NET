namespace LibraryManagement.DTO.Response;

public class MonthlyStatDto
{
    public string Label { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
    public int ReturnCount { get; set; }
}
namespace LibraryManagement.DTO.Response;

public class FineDetailDto
{
    public string FineCode { get; set; } = string.Empty;
    public string LoanCode { get; set; } = string.Empty;
    public string ReaderName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public decimal AmountVnd { get; set; }
}
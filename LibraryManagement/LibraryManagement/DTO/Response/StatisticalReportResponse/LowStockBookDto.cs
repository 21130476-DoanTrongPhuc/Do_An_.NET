namespace LibraryManagement.DTO.Response;

public class LowStockBookDto
{
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int BorrowedCopies { get; set; }
    public int AvailableCopies { get; set; }
    public string StatusText { get; set; } = string.Empty;
}
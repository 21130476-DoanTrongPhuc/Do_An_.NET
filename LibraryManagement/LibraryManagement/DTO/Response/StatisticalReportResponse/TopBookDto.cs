namespace LibraryManagement.DTO.Response;

public class TopBookDto
{
    public int Rank { get; set; }
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
    public int AvailableCopies { get; set; }
    public int TotalCopies { get; set; }
}
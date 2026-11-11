namespace LibraryManagement.DTO;

public class BookDto
{
    public string BookCode { get; set; } = string.Empty;
    public string? ISBN { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int? SoLuongB { get; set; }
}
namespace LibraryManagement.Models;

public class Book
{
    public int Id { get; set; }
    public int IdCategory { get; set; }
    public string BookCode { get; set; } = string.Empty;
    public string? ISBN { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? PublishedYear { get; set; }
    public string? Description { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;

    public Categories Category { get; set; } = null;
    public ICollection<BookCopies> Copies { get; set; } = new List<BookCopies>();
    public ICollection<Reservations>  Reservations { get; set; } = new List<Reservations>();
}
namespace LibraryManagement.Models;

public class Book
{
    public int Id { get; set; }
    public int IdCategory { get; set; }
    public string BookCode { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public int PublishedYear { get; set; }
    public string? Language { get; set; } = "Tiếng Việt";
    public string? CoverImagePath { get; set; }
    public string Status { get; set; } = "Active"; // Active | Discontinued
    // public int? PublishedYear { get; set; }         // ← int? (SQL INTEGER)
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Categories Category { get; set; } = null!;
    public ICollection<BookCopies> Copies { get; set; } = new List<BookCopies>();
    public ICollection<Reservations> Reservations { get; set; } = new List<Reservations>();
}
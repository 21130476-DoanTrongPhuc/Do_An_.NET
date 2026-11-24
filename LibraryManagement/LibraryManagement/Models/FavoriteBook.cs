namespace LibraryManagement.Models;

public class FavoriteBook
{
    public int ReaderId { get; set; }
    public int BookId { get; set; }
    public DateTime AddedDate { get; set; } = DateTime.Now;

    // Navigation
    public Users Reader { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
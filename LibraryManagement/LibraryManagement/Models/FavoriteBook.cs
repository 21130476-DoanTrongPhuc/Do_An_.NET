namespace LibraryManagement.Models;

public class FavoriteBook
{
    public int ReaderId { get; set; }
    public int BookId { get; set; }
    public DateTime AddedDate { get; set; } = DateTime.Now;
}
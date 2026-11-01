namespace LibraryManagement.Models;

public class Book
{
    public int Id { get; set; }

    public string ISBN { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public int AvailableQuantity { get; set; }
}
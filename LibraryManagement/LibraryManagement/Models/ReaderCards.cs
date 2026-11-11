namespace LibraryManagement.Models;

public class ReaderCards
{
    public int Id { get; set; }
    public int IdReader  { get; set; }
    public string CardCode { get; set; } = string.Empty;
    public string IssuedAt { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
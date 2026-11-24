namespace LibraryManagement.Models;

public class ReaderCards
{
    public int Id { get; set; }
    public int IdReader { get; set; }
    public string CardCode { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }               // ← nullable

    // Navigation
    public Users Reader { get; set; } = null!;
}
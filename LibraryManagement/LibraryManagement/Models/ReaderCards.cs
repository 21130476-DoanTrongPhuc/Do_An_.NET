using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class ReaderCards
{
    public int Id { get; set; }
    public int IdReader { get; set; }
    public string CardCode { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public ReaderCardStatus Status { get; set; } = ReaderCardStatus.Active;
    public string? Note { get; set; }

    // Navigation
    public Users Reader { get; set; } = null!;
}
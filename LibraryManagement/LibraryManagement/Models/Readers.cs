namespace LibraryManagement.Models;

public class Readers
{
    public int Id { get; set; }
    public DateTime IsCardExpiryDate { get; set; }
    public string? ReaderId { get; set; }
    public string ReaderCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime CardExpiryDate { get; set; }
    public string Password { get; set; } = "123456";
    public bool IsLocked { get; set; }
}
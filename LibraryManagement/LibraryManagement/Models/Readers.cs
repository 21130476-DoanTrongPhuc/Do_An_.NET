namespace LibraryManagement.Models;

public class Readers
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
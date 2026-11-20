namespace LibraryManagement.Models;

public class Users
{
    public int Id { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string UserType { get; set; } = string.Empty;
    public int? MaxActiveCopies { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } 
    public DateTime UpdatedAt { get; set; } 

    // Navigation Properties (Chỉ giữ lại phần của Reader khớp với SQL)
    public ICollection<ReaderCards> ReaderCards { get; set; } = new List<ReaderCards>();
    public ICollection<Loans> BorrowedLoans { get; set; } = new List<Loans>();
    public ICollection<Reservations> Reservations { get; set; } = new List<Reservations>();
}
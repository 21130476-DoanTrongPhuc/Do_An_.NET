using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class Users
{
    public int Id { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public UserType UserType { get; set; } 
    public int? MaxActiveCopies { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<ReaderCards> ReaderCards { get; set; } = new List<ReaderCards>();
    public ICollection<Loans> BorrowedLoans { get; set; } = new List<Loans>();
    public ICollection<Reservations> Reservations { get; set; } = new List<Reservations>();
}
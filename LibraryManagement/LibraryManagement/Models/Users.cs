namespace LibraryManagement.Models;

public class Users
{
    // Primary Key
    public int Id { get; set; }

    // Basic Information
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    // Authentication
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }

    // Personal Information
    public string? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    // User Classification
    public string UserType { get; set; } = string.Empty;

    // Reader-specific Information
    public int? MaxActiveCopies { get; set; }

    // Account Status
    public string Status { get; set; } = "Active";

    // Timestamps
    public DateTime CreatedAt { get; set; } 
    public DateTime UpdatedAt { get; set; } 

    // Navigation Properties

    // As a Reader
    public ICollection<ReaderCards> ReaderCards { get; set; }
        = new List<ReaderCards>();

    public ICollection<Loans> BorrowedLoans { get; set; }
        = new List<Loans>();

    public ICollection<Reservations> Reservations { get; set; }
        = new List<Reservations>();

    // As a Librarian
    public ICollection<Loans> ProcessedLoans { get; set; }
        = new List<Loans>();

    public ICollection<LoanDetails> ReturnedLoanDetails { get; set; }
        = new List<LoanDetails>();

    public ICollection<Reservations> ProcessedReservations { get; set; }
        = new List<Reservations>();

    public ICollection<Fines> CreatedFines { get; set; }
        = new List<Fines>();

    public ICollection<FinePayments> ProcessedFinePayments { get; set; }
        = new List<FinePayments>();
}
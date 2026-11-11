namespace LibraryManagement.Models;

public class Librarians
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    
    public ICollection<Loans>  Loans { get; set; } = new List<Loans>();
    public ICollection<LoanDetails> ReturnedLoanDetails { get; set; } = new List<LoanDetails>();
    
    public ICollection<Reservations> Reservations { get; set; } = new List<Reservations>();
    public ICollection<Fines> Fines { get; set; } = new List<Fines>();
    public ICollection<FinePayments> FinePayments { get; set; } = new List<FinePayments>();
}
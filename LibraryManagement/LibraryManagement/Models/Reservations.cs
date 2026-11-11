namespace LibraryManagement.Models;

public class Reservations
{
    public int Id { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public int IdReader { get; set; } 
    public int IdBook { get; set; } 
    public int? IdBookCopies { get; set; } 
    public int? IdLibrarian { get; set; } 
    public int? IdLoan { get; set; } 
    public string ReservedAt  { get; set; } = string.Empty;
    public string? ExpireAt  { get; set; }
    public string? AssignedAt  { get; set; }
    public string? FulfilledAt  { get; set; }
    public string? CancelledAt  { get; set; }
    public string Status  { get; set; } = string.Empty;
    public string? Notes  { get; set; }
    
    public Readers Reader { get; set; } = null!;
    public Book Book { get; set; } = null!;
    public BookCopies? BookCopy { get; set; }
    public Librarians? Librarian { get; set; }
    public Loans? Loan { get; set; }
}
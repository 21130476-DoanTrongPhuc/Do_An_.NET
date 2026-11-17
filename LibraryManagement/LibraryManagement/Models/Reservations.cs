namespace LibraryManagement.Models;

public class Reservations
{
    public int Id { get; set; }
    public string ReservationCode { get; set; }
    public int IdReader { get; set; }
    public int IdBook { get; set; }
    public int? IdBookCopy { get; set; }
    public int? IdLoan { get; set; }
    public DateTime ReservedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? FulfilledAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string Status { get; set; }
    public string Note { get; set; }

    // Navigation
    public Users Reader { get; set; }
    public Book Book { get; set; }
    public BookCopies BookCopy { get; set; }
    public Loans Loan { get; set; }
    
}
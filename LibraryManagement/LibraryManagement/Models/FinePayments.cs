namespace LibraryManagement.Models;

public class FinePayments
{
    public int Id { get; set; }
    public string PaymentCode { get; set; } = null;
    public int IdFine { get; set; }
    public int IdLibrarian { get; set; }
    public string PaidAt { get; set; } = null;
    public int AmountVnd { get; set; } = 0;
    public string PaymentMethod { get; set; } = null;
    public string? Note  { get; set; } 
    
    public Fines  Fines { get; set; } = null;
    public Librarians Librarian { get; set; } = null;
}
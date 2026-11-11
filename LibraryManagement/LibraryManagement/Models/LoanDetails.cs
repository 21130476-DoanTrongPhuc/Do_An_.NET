namespace LibraryManagement.Models;

public class LoanDetails
{
    public int Id { get; set; }
    public int LibraryId { get; set; }
    public int IdBookCopy { get; set; }
    public string DueAt { get; set; } = string.Empty;
    public string? ReturnedAt { get; set; }
    public string PhysicalConditionBefore { get; set; } = string.Empty;
    public string PhysicalConditionAfter { get; set; }
    public int? IdLibrarianReturned { get; set; }
    public int RenewalCount { get; set; }
    public string? Note  { get; set; }
    
    public Loans Loans  { get; set; } = null;
    public BookCopies BookCopies { get; set; } = null;
    public Librarians? LibrariansReturned { get; set; } = null;
    public ICollection<Fines> Fines { get; set; } = new List<Fines>();
}
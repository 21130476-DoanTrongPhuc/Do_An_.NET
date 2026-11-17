namespace LibraryManagement.Models;

using System;

public class LoanDetails
{
    public int Id { get; set; }
    public int IdLoan { get; set; }
    public int IdBookCopy { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string PhysicalConditionBefore { get; set; }
    public string PhysicalConditionAfter { get; set; }
    public int RenewalCount { get; set; }
    public string Note { get; set; }

    // Navigation
    public Loans Loan { get; set; }
    public BookCopies BookCopy { get; set; }
    public ICollection<Fines> Fines { get; set; }
}
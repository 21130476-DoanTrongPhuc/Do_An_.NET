namespace LibraryManagement.Models;

using System;

public class LoanDetails
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public string BookCopyBarcode { get; set; } = string.Empty;
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = "Đang mượn";
    public Loans Loan { get; set; } = null!;
    public BookCopies BookCopy { get; set; } = null!;
}
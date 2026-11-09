namespace LibraryManagement.Models;

public class BorrowRecord
{
    public int Id { get; set; }
    public int ReaderId { get; set; }
    public int BookId { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }        
    public DateTime? ReturnDate { get; set; }      

    public Book? Book { get; set; }              

    public bool IsReturned => ReturnDate != null;
    public int DaysLeft => (DueDate.Date - DateTime.Today).Days;   
    public bool IsOverdue => !IsReturned && DaysLeft < 0;
}
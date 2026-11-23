using LibraryManagement.Enums;

namespace LibraryManagement.DTO.Response;

public class ManageOverdueBookInfo
{
    public int IdLoanDetail { get; set; }
    public string LoanCode { get; set; }
    public string UserCode { get; set; }
    public string FullName { get; set; } 
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string CopyCode { get; set; }
    public string Title { get; set; }
    public DateTime BorrowedDay  { get; set; }
    public DateTime DueDay { get; set; }
    public int OverdueDays { get; set; }
    public double FinePay { get; set; }
    public ReminderEmailPhone  ReminderStatus { get; set; }
    public DateTime? LatestReminderAt { get; set; }
}
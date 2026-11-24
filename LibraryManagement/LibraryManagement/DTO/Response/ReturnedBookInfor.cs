using LibraryManagement.Enums;

namespace LibraryManagement.DTO.Response;

public class ReturnedBookInfor
{
    public string CopyCode { get; set; } = string.Empty;
    public int LoanDetailId  { get; set; }
    public string LoanCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ISBN { get; set; }
    public BookCondition PhysicalConditionBefore { get; set; }
    public BookCondition PhysicalConditionAfter { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public int NumberDaysLate { get; set; } 
    public double AmountPayDaysLate { get; set; } 
}
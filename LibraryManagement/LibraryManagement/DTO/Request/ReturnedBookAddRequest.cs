using LibraryManagement.Enums;

namespace LibraryManagement.DTO.Request;

public class ReturnedBookAddRequest
{
    public int LoanDetailId { get; set; }
    public BookCondition PhysicalConditionAfter { get; set; }
    public DateTime? ReturnedAt { get; set; }
}
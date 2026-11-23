using LibraryManagement.Enums;

namespace LibraryManagement.DTO.Request;

public class CreateFineManagementRequest
{
    public int LoanDetaiId  { get; set; }
    public string UserCode   { get; set; }
    public string FullName  { get; set; }
    public string LoanCode  { get; set; }
    public ViolationType ViolationType { get; set; }
    public double FineAmount { get; set; }
    public string Note { get; set; }
}
using LibraryManagement.Enums;

namespace LibraryManagement.DTO.Request;

public class UpdateFineManagementRequest
{
    public int Id { get; set; }
    public int LoanDetailId { get; set; }
    public ViolationType ViolationType { get; set; }
    public int FineAmount { get; set; }
    public string Note { get; set; } = "";
}
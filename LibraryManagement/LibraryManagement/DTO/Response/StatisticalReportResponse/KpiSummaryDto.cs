namespace LibraryManagement.DTO.Response;

public class KpiSummaryDto
{
    public int TotalBooks { get; set; }
    public int AvailableCopies { get; set; }
    public int TotalCopies { get; set; }
    public int BorrowedInRange { get; set; }
    public int ReturnedInRange { get; set; }
    public int CurrentOverdue { get; set; }
    public decimal TotalFineAmount { get; set; }
    public int TotalFineCount { get; set; }
}
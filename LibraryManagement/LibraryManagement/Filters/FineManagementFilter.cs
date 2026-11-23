using LibraryManagement.Enums;

namespace LibraryManagement.Filters;

public class FineManagementFilter
{
    public string? KeyWord { get; set; }
    
    public ViolationType ViolationType { get; set; }
    
    public FineStatus FineStatus  { get; set; }
    
    public DateTime  StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
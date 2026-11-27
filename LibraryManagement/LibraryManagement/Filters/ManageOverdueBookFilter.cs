using LibraryManagement.Enums;

namespace LibraryManagement.DTO.Request;

public sealed record ManageOverdueBookFilter
{
    public string? KeyWord { get; set; }

    public OverduePeriod OverduePeriod { get; set; } = OverduePeriod.All;
    
    public DateTime? DueFrom { get; init; }
    public DateTime? DueTo { get; init; }
    
    public DateTime? BorrowedFrom { get; init; }
    public DateTime? BorrowedTo { get; init; }

    public ReminderFilterStatus? Status { get; init; } = ReminderFilterStatus.All;

}
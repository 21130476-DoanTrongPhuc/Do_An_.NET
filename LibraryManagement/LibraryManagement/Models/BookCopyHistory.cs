using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class BookCopyHistory
{
    public int Id { get; set; }
    public int IdBookCopy { get; set; }
    public string ChangedAt { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public CirculationStatus? OldStatus { get; set; }
    public CirculationStatus? NewStatus { get; set; }
    public BookCondition? OldCondition { get; set; }
    public BookCondition? NewCondition { get; set; }
    public string? OldLocation { get; set; }
    public string? NewLocation { get; set; }
    public string? Note { get; set; }
    
    public BookCopies BookCopy { get; set; } = null!;
}
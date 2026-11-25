using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class Reminders
{
    public int Id { get; set; }
    public int IdLoanDetail { get; set; }

    public ReminderChannel Channel { get; set; }
    public ReminderStatus Status { get; set; } = ReminderStatus.Pending;

    public DateTime? SentAt { get; set; }
    public string? Content { get; set; }
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public LoanDetails LoanDetail { get; set; } = null!;
}
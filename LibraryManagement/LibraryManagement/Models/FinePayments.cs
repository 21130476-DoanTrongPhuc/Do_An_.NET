using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class FinePayments
{
    public int Id { get; set; }
    public string? PaymentCode { get; set; }        // SQL NOT NULL — code phải set trước khi insert
    public int IdFine { get; set; }
    public DateTime PaidAt { get; set; }
    public int AmountVnd { get; set; }              // ← int (SQL INTEGER)
    public PaymentMethod PaymentMethod { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }         // ← thêm (SQL có cột này)

    // Navigation
    public Fines Fine { get; set; } = null!;
}
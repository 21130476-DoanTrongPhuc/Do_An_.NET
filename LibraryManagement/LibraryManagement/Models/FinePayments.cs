using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class FinePayments
{
    public int Id { get; set; }
    public string? PaymentCode { get; set; }
    public int IdFine { get; set; }
    public DateTime PaidAt { get; set; }             // đổi thành bắt buộc
    public int AmountVnd { get; set; }
    public PaymentMethod PaymentMethod { get; set; } // đổi thành bắt buộc
    public string? Note { get; set; }

    // Navigation
    public Fines Fine { get; set; } = null!;
}
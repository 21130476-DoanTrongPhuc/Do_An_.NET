using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class FinePayments
{
    public int Id { get; set; }
    public string? PaymentCode { get; set; }
    public int IdFine { get; set; }
    public DateTime? PaidAt { get; set; }
    public int AmountVnd { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? Note { get; set; }

    // Navigation
    public Fines Fine { get; set; }
}
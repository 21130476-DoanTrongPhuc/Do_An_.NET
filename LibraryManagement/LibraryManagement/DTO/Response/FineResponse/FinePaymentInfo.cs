using LibraryManagement.Enums;

namespace LibraryManagement.DTO.Response;

public class FinePaymentInfo
{
    public int No { get; set; }
    public DateTime PaidAt { get; set; }
    public int AmountVnd { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Note { get; set; }
    public string? PaymentCode { get; set; }
}
namespace LibraryManagement.DTO.Response;

public class ReservationDetailResponse
{
    public int Id { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public string ReaderFullName { get; set; } = string.Empty;
    public string ReaderCode { get; set; } = string.Empty;
    public string ReaderContact { get; set; } = string.Empty;
    public string BookCode { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Note { get; set; }

    public bool IsPending { get; set; }
    public bool IsReadyForPickup { get; set; }
    public bool HasAssignedCopy { get; set; }

    public List<ReservationCopyOptionResponse> CopyOptions { get; set; } = new();
}
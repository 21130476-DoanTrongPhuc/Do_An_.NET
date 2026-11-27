namespace LibraryManagement.DTO.Response;

public class ReservationListItemResponse
{
    public int Id { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public string ReaderDisplay { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string ReservedAtText { get; set; } = string.Empty;
    public string AvailableText { get; set; } = string.Empty;
    public string AssignedCopyCode { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
}
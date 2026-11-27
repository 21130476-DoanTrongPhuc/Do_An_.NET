namespace LibraryManagement.Filters;

public class ReservationFilter
{
    public string? Keyword { get; set; }
    public int StatusFilterIndex { get; set; } = 1;
}
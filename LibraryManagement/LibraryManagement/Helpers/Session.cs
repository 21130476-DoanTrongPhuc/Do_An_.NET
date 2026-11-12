namespace LibraryManagement.Helpers;

public static class Session
{
    public static int CurrentLibrarianId { get; set; }
    public static int CurrentReaderId { get; set; }

    public static void Clear()
    {
        CurrentLibrarianId = 0;
        CurrentReaderId = 0;
    }
}
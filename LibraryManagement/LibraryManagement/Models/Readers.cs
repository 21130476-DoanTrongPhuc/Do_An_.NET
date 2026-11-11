namespace LibraryManagement.Models;

public class Readers
{
        
    public DateTime IsCardExpiryDate { get; set; } // Ngày hết hạn thẻ

    public string? ReaderId { get; set; }
    public string ReaderCode { get; set; } = "";  
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime CardExpiryDate { get; set; }   
    
}
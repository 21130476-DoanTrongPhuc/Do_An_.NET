namespace LibraryManagement.Models;

public class Reservations
{
    public int Id { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    
    // --- Khóa ngoại ---
    public int IdReader { get; set; } 
    public int IdBook { get; set; } 
    public int? IdBookCopy { get; set; } // Đã sửa lại thành IdBookCopy (số ít) cho đúng chuẩn
    public int? IdLibrarian { get; set; } 
    public int? IdLoan { get; set; } 
    
    // --- Thông tin khác ---
    public string ReservedAt  { get; set; } = string.Empty;
    public string? ExpiresAt  { get; set; } // Đã sửa ExpireAt thành ExpiresAt cho giống DB
    public string? AssignedAt  { get; set; }
    public string? FulfilledAt  { get; set; }
    public string? CancelledAt  { get; set; }
    public string Status  { get; set; } = string.Empty;
    public string? Note  { get; set; } // Đã sửa Notes thành Note cho giống DB
    
    // --- Navigation Properties ---
    // XÓA thuộc tính "Users" chung chung đi
    
    public Users Reader { get; set; } = null!;       // Người đặt trước
    public Users? Librarian { get; set; }            // Thủ thư xử lý (có thể null)
    
    public Book Book { get; set; } = null!;         // Đã sửa Book thành Books cho khớp với tên class của bạn
    public BookCopies? BookCopy { get; set; }        // Cuốn sách cụ thể được gán (có thể null)
    public Loans? Loan { get; set; }                 // Phiếu mượn liên kết (có thể null)
}
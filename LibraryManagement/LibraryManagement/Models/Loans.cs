using System;
using System.Collections.Generic;

namespace LibraryManagement.Models;

public class Loans
{
    public int Id { get; set; }
    
    // Độc giả
    public int IdReader { get; set; }
    public Users Reader { get; set; } = null!;      

    // Thủ thư
    public int IdLibrarian { get; set; }
    public Users Librarian { get; set; } = null!;   
    
    // Thông tin khác
    public DateTime BorrowDate { get; set; }
    public string Status { get; set; } = string.Empty; 

    // Navigation properties
    public ICollection<LoanDetails> LoanDetails { get; set; } = new List<LoanDetails>();
}
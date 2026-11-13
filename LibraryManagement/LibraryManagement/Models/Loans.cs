using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;
public class Loans
{
    [Key]
    public int Id { get; set; }
    public int ReaderId { get; set; }
    public int LibrarianId { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty; // Đang mượn, Đã trả một phần, Đã trả đủ, Quá hạn

    // Navigation properties
    public virtual Readers Reader { get; set; } = null!;
    public virtual Librarians Librarian { get; set; } = null!;
    public virtual ICollection<LoanDetails> LoanDetails { get; set; } = new List<LoanDetails>();
}
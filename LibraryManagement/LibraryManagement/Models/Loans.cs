using System;
using System.Collections.Generic;

namespace LibraryManagement.Models;

public class Loans
{
    public int Id { get; set; }
    public string LoanCode { get; set; }
    public int IdReader { get; set; }
    public DateTime BorrowedAt { get; set; }
    public string Note { get; set; }

    // Navigation
    public Users Reader { get; set; }
    public ICollection<LoanDetails> LoanDetails { get; set; }
}
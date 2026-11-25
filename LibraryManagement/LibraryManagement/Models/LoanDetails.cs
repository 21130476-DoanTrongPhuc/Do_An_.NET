using System;
using System.ComponentModel.DataAnnotations;
using LibraryManagement.Enums;

namespace LibraryManagement.Models;

public class LoanDetails
{
    public int Id { get; set; }
    public int IdLoan { get; set; }
    public int IdBookCopy { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }

    public BookCondition PhysicalConditionBefore { get; set; }   // ← enum (SQL CHECK 4 giá trị)
    public BookCondition? PhysicalConditionAfter { get; set; }

    public int RenewalCount { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    // Navigation
    public Loans Loan { get; set; } = null!;
    public BookCopies BookCopy { get; set; } = null!;
    public ICollection<Fines> Fines { get; set; } = new List<Fines>();
    public ICollection<Reminders> Reminders { get; set; } = new List<Reminders>();
}
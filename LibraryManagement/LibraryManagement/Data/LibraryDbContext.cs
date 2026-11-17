using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext()
    {
    }

    public LibraryDbContext(
        DbContextOptions<LibraryDbContext> options
    ) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=library.db");
        }
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<Categories> BookCategories { get; set; }
    public DbSet<BookCopies> BookCopies { get; set; }
    public DbSet<Users> Users { get; set; }
    public DbSet<ReaderCards> ReaderCards { get; set; }
    public DbSet<Loans> Loans { get; set; }
    public DbSet<LoanDetails> LoanDetails { get; set; }
    public DbSet<Reservations> Reservations { get; set; }
    public DbSet<Fines> Fines { get; set; }
    public DbSet<FinePayments> FinePayments { get; set; }
    
   protected override void OnModelCreating(ModelBuilder modelBuilder) 
   {
        base.OnModelCreating(modelBuilder);

        // =====================================================
        // 1. USERS
        // =====================================================
        modelBuilder.Entity<Users>().ToTable("Users");
        modelBuilder.Entity<Users>().HasKey(u => u.Id);

        // =====================================================
        // 2. BOOK CATEGORIES
        // =====================================================
        modelBuilder.Entity<Categories>().ToTable("BookCategories");
        modelBuilder.Entity<Categories>().HasKey(c => c.Id);

        // =====================================================
        // 3. BOOKS
        // =====================================================
        modelBuilder.Entity<Book>().ToTable("Books");
        modelBuilder.Entity<Book>().HasKey(b => b.Id);

        modelBuilder.Entity<Book>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.IdCategory)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 4. BOOK COPIES
        // =====================================================
        modelBuilder.Entity<BookCopies>().ToTable("BookCopies");
        modelBuilder.Entity<BookCopies>().HasKey(c => c.Id);

        modelBuilder.Entity<BookCopies>()
            .HasOne(c => c.Book)
            .WithMany(b => b.Copies)
            .HasForeignKey(c => c.IdBook)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 5. READER CARDS
        // =====================================================
        modelBuilder.Entity<ReaderCards>().ToTable("ReaderCards");
        modelBuilder.Entity<ReaderCards>().HasKey(c => c.Id);

        modelBuilder.Entity<ReaderCards>()
            .HasOne(c => c.Reader)
            .WithMany(u => u.ReaderCards)
            .HasForeignKey(c => c.IdReader)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 6. LOANS (Đã bỏ Librarian)
        // =====================================================
        modelBuilder.Entity<Loans>().ToTable("Loans");
        modelBuilder.Entity<Loans>().HasKey(l => l.Id);
        modelBuilder.Entity<Loans>().Property(l => l.BorrowedAt).HasColumnName("BorrowedAt");

        modelBuilder.Entity<Loans>()
            .HasOne(l => l.Reader)
            .WithMany(u => u.BorrowedLoans)
            .HasForeignKey(l => l.IdReader)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 7. LOAN DETAILS (Đã bỏ LibrarianReturn)
        // =====================================================
        modelBuilder.Entity<LoanDetails>().ToTable("LoanDetails");
        modelBuilder.Entity<LoanDetails>().HasKey(d => d.Id);

        modelBuilder.Entity<LoanDetails>()
            .HasOne(d => d.Loan)
            .WithMany(l => l.LoanDetails)
            .HasForeignKey(d => d.IdLoan)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LoanDetails>()
            .HasOne(d => d.BookCopy)
            .WithMany(c => c.LoanDetails)
            .HasForeignKey(d => d.IdBookCopy)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 8. RESERVATIONS (Đã bỏ Librarian)
        // =====================================================
        modelBuilder.Entity<Reservations>().ToTable("Reservations");
        modelBuilder.Entity<Reservations>().HasKey(r => r.Id);

        modelBuilder.Entity<Reservations>()
            .HasOne(r => r.Reader)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.IdReader)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reservations>()
            .HasOne(r => r.Book)
            .WithMany()
            .HasForeignKey(r => r.IdBook)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reservations>()
            .HasOne(r => r.BookCopy)
            .WithMany()
            .HasForeignKey(r => r.IdBookCopy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reservations>()
            .HasOne(r => r.Loan)
            .WithMany()
            .HasForeignKey(r => r.IdLoan)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reservations>().Property(r => r.ExpiresAt).HasColumnName("ExpiresAt");
        modelBuilder.Entity<Reservations>().Property(r => r.Note).HasColumnName("Note");

        // =====================================================
        // 9. FINES (Đã bỏ Librarian)
        // =====================================================
        modelBuilder.Entity<Fines>().ToTable("Fines");
        modelBuilder.Entity<Fines>().HasKey(f => f.Id);

        modelBuilder.Entity<Fines>()
            .HasOne(f => f.LoanDetail)
            .WithMany(d => d.Fines)
            .HasForeignKey(f => f.IdLoanDetail)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 10. FINE PAYMENTS (Đã bỏ Librarian)
        // =====================================================
        modelBuilder.Entity<FinePayments>().ToTable("FinePayments");
        modelBuilder.Entity<FinePayments>().HasKey(p => p.Id);

        modelBuilder.Entity<FinePayments>()
            .HasOne(p => p.Fine)
            .WithMany(f => f.FinePayments)
            .HasForeignKey(p => p.IdFine)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
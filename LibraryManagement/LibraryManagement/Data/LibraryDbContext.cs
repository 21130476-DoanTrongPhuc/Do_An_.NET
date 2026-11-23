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
            optionsBuilder.UseSqlite(DatabaseConnection.GetConnectionString());
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
    public DbSet<Reminders> Reminders  { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Users>().ToTable("Users");
        modelBuilder.Entity<Users>().HasKey(u => u.Id);

        modelBuilder.Entity<Categories>().ToTable("BookCategories");
        modelBuilder.Entity<Categories>().HasKey(c => c.Id);

        modelBuilder.Entity<Book>().ToTable("Books");
        modelBuilder.Entity<Book>().HasKey(b => b.Id);

        modelBuilder.Entity<Book>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.IdCategory)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BookCopies>().ToTable("BookCopies");
        modelBuilder.Entity<BookCopies>().HasKey(c => c.Id);

        modelBuilder.Entity<BookCopies>()
            .HasOne(c => c.Book)
            .WithMany(b => b.Copies)
            .HasForeignKey(c => c.IdBook)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReaderCards>().ToTable("ReaderCards");
        modelBuilder.Entity<ReaderCards>().HasKey(c => c.Id);

        modelBuilder.Entity<ReaderCards>()
            .HasOne(c => c.Reader)
            .WithMany(u => u.ReaderCards)
            .HasForeignKey(c => c.IdReader)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Loans>().ToTable("Loans");
        modelBuilder.Entity<Loans>().HasKey(l => l.Id);
        modelBuilder.Entity<Loans>().Property(l => l.BorrowedAt).HasColumnName("BorrowedAt");

        modelBuilder.Entity<Loans>()
            .HasOne(l => l.Reader)
            .WithMany(u => u.BorrowedLoans)
            .HasForeignKey(l => l.IdReader)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LoanDetails>().ToTable("LoanDetails");
        modelBuilder.Entity<LoanDetails>().HasKey(d => d.Id);
        modelBuilder.Entity<LoanDetails>()
            .Property(d => d.PhysicalConditionAfter)
            .HasConversion<string>();

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

        modelBuilder.Entity<Fines>().ToTable("Fines");
        modelBuilder.Entity<Fines>().HasKey(f => f.Id);

        modelBuilder.Entity<Fines>()
            .HasOne(f => f.LoanDetail)
            .WithMany(d => d.Fines)
            .HasForeignKey(f => f.IdLoanDetail)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FinePayments>().ToTable("FinePayments");
        modelBuilder.Entity<FinePayments>().HasKey(p => p.Id);

        modelBuilder.Entity<FinePayments>()
            .HasOne(p => p.Fine)
            .WithMany(f => f.FinePayments)
            .HasForeignKey(p => p.IdFine)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Reminders>()
            .HasOne(e => e.LoanDetail)
            .WithMany(ld => ld.Reminders)
            .HasForeignKey(e => e.IdLoanDetail)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Reminders>().ToTable("Reminders");
        modelBuilder.Entity<Reminders>().HasKey(r => r.Id);

        modelBuilder.Entity<Reminders>()
            .Property(r => r.Channel)
            .HasConversion<string>()          // Email / SMS
            .IsRequired();

        modelBuilder.Entity<Reminders>()
            .Property(r => r.Status)
            .HasConversion<string>()          // Pending / Sent / Failed
            .IsRequired();

        modelBuilder.Entity<Reminders>()
            .Property(r => r.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Reminders>()
            .Property(r => r.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Reminders>()
            .HasOne(r => r.LoanDetail)
            .WithMany(ld => ld.Reminders)
            .HasForeignKey(r => r.IdLoanDetail)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
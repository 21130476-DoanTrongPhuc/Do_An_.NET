using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext()
    {
    }
    public DbSet<BookCopyHistory> BookCopyHistories { get; set; }
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
    public DbSet<FavoriteBook> FavoriteBooks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Users (bỏ qua các collection của Librarian vì DB không còn cột IdLibrarian)
        modelBuilder.Entity<Users>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Ignore(u => u.ProcessedLoans);
            entity.Ignore(u => u.ReturnedLoanDetails);
            entity.Ignore(u => u.ProcessedReservations);
            entity.Ignore(u => u.CreatedFines);
            entity.Ignore(u => u.ProcessedFinePayments);
        });

        // 2. BookCategories (ánh xạ IdParentCategory -> IdParent và bỏ qua Name)
        modelBuilder.Entity<Categories>(entity =>
        {
            entity.ToTable("BookCategories");
            entity.HasKey(c => c.Id);
            entity.Ignore(c => c.Name);

            entity.Property(c => c.IdParentCategory)
                .HasColumnName("IdParent");

            entity.HasOne(c => c.ParentCategory)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.IdParentCategory)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 3. Books
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("Books");
            entity.HasKey(b => b.Id);

            entity.HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.IdCategory)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 4. BookCopies (ánh xạ PurchasePrice -> PurchasePriceVnd, Notes -> Note)
        modelBuilder.Entity<BookCopies>(entity =>
        {
            entity.ToTable("BookCopies");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.PurchasePrice)
                .HasColumnName("PurchasePriceVnd");
            entity.Property(c => c.Notes)
                .HasColumnName("Note");

            entity.HasOne(c => c.Book)
                .WithMany(b => b.Copies)
                .HasForeignKey(c => c.IdBook)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 5. ReaderCards
        modelBuilder.Entity<ReaderCards>(entity =>
        {
            entity.ToTable("ReaderCards");
            entity.HasKey(c => c.Id);

            entity.HasOne(c => c.Reader)
                .WithMany(u => u.ReaderCards)
                .HasForeignKey(c => c.IdReader)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 6. Loans
        modelBuilder.Entity<Loans>(entity =>
        {
            entity.ToTable("Loans");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.BorrowedAt).HasColumnName("BorrowedAt");

            entity.HasOne(l => l.Reader)
                .WithMany(u => u.BorrowedLoans)
                .HasForeignKey(l => l.IdReader)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 7. LoanDetails
        modelBuilder.Entity<LoanDetails>(entity =>
        {
            entity.ToTable("LoanDetails");
            entity.HasKey(d => d.Id);

            entity.HasOne(d => d.Loan)
                .WithMany(l => l.LoanDetails)
                .HasForeignKey(d => d.IdLoan)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.BookCopy)
                .WithMany(c => c.LoanDetails)
                .HasForeignKey(d => d.IdBookCopy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 8. Reservations
        modelBuilder.Entity<Reservations>(entity =>
        {
            entity.ToTable("Reservations");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.ExpiresAt).HasColumnName("ExpiresAt");
            entity.Property(r => r.Note).HasColumnName("Note");

            entity.HasOne(r => r.Reader)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.IdReader)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Book)
                .WithMany(b => b.Reservations)
                .HasForeignKey(r => r.IdBook)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.BookCopy)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.IdBookCopy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Loan)
                .WithMany()
                .HasForeignKey(r => r.IdLoan)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 9. Fines
        modelBuilder.Entity<Fines>(entity =>
        {
            entity.ToTable("Fines");
            entity.HasKey(f => f.Id);
            entity.Ignore(f => f.Status);

            entity.HasOne(f => f.LoanDetail)
                .WithMany(d => d.Fines)
                .HasForeignKey(f => f.IdLoanDetail)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 10. FinePayments
        modelBuilder.Entity<FinePayments>(entity =>
        {
            entity.ToTable("FinePayments");
            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.Fine)
                .WithMany(f => f.FinePayments)
                .HasForeignKey(p => p.IdFine)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 11. FavoriteBooks
        modelBuilder.Entity<FavoriteBook>(entity =>
        {
            entity.ToTable("FavoriteBooks");
            entity.HasKey(f => new { f.ReaderId, f.BookId });
            entity.Property(f => f.ReaderId).HasColumnName("IdReader");
            entity.Property(f => f.BookId).HasColumnName("IdBook");
            entity.Property(f => f.AddedDate).HasColumnName("AddedAt");

            entity.HasOne(f => f.Reader)
                .WithMany()
                .HasForeignKey(f => f.ReaderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Book)
                .WithMany()
                .HasForeignKey(f => f.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
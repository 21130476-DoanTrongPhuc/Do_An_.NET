using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext() { }

    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }
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
            optionsBuilder.UseSqlite(DatabaseConnection.GetConnectionString());
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
    public DbSet<Reminders> Reminders { get; set; }
    public DbSet<FineAuditLogs> FineAuditLogs { get; set; }

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
        // ==================== USERS ====================
        modelBuilder.Entity<Users>().ToTable("Users");
        modelBuilder.Entity<Users>().HasKey(u => u.Id);
        modelBuilder.Entity<Users>().Property(u => u.UserType).HasConversion<string>();
        modelBuilder.Entity<Users>().Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Users>().Property(u => u.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        // ==================== CATEGORIES ====================
        modelBuilder.Entity<Categories>().ToTable("BookCategories");
        modelBuilder.Entity<Categories>().HasKey(c => c.Id);
        modelBuilder.Entity<Categories>().Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Categories>().Property(c => c.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Categories>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.IdParent)
            .OnDelete(DeleteBehavior.Restrict);
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
        // ==================== BOOKS ====================
        modelBuilder.Entity<Book>().ToTable("Books");
        modelBuilder.Entity<Book>().HasKey(b => b.Id);
        modelBuilder.Entity<Book>().Property(b => b.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Book>().Property(b => b.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.IdCategory)
            .OnDelete(DeleteBehavior.Restrict);

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
        // ==================== BOOK COPIES ====================
        modelBuilder.Entity<BookCopies>().ToTable("BookCopies");
        modelBuilder.Entity<BookCopies>().HasKey(c => c.Id);
        modelBuilder.Entity<BookCopies>().Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<BookCopies>().Property(c => c.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<BookCopies>()
            .Property(c => c.PhysicalCondition)
            .HasConversion<string>();
        modelBuilder.Entity<BookCopies>()
            .Property(c => c.CirculationStatus)
            .HasConversion<string>();
        modelBuilder.Entity<BookCopies>()
            .HasOne(c => c.Book)
            .WithMany(b => b.Copies)
            .HasForeignKey(c => c.IdBook)
            .OnDelete(DeleteBehavior.Restrict);

        // ==================== READER CARDS ====================
        modelBuilder.Entity<ReaderCards>().ToTable("ReaderCards");
        modelBuilder.Entity<ReaderCards>().HasKey(c => c.Id);
        modelBuilder.Entity<ReaderCards>()
            .HasOne(c => c.Reader)
            .WithMany(u => u.ReaderCards)
            .HasForeignKey(c => c.IdReader)
            .OnDelete(DeleteBehavior.Restrict);
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
        // ==================== LOANS ====================
        modelBuilder.Entity<Loans>().ToTable("Loans");
        modelBuilder.Entity<Loans>().HasKey(l => l.Id);
        modelBuilder.Entity<Loans>().Property(l => l.BorrowedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Loans>()
            .HasOne(l => l.Reader)
            .WithMany(u => u.BorrowedLoans)
            .HasForeignKey(l => l.IdReader)
            .OnDelete(DeleteBehavior.Restrict);

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
        // ==================== LOAN DETAILS ====================
        modelBuilder.Entity<LoanDetails>().ToTable("LoanDetails");
        modelBuilder.Entity<LoanDetails>().HasKey(d => d.Id);
        modelBuilder.Entity<LoanDetails>()
            .Property(d => d.PhysicalConditionBefore).HasConversion<string>();
        modelBuilder.Entity<LoanDetails>()
            .Property(d => d.PhysicalConditionAfter).HasConversion<string>();
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

        // ==================== RESERVATIONS ====================
        modelBuilder.Entity<Reservations>().ToTable("Reservations");
        modelBuilder.Entity<Reservations>().HasKey(r => r.Id);
        modelBuilder.Entity<Reservations>().Property(r => r.ReservedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Reservations>()
            .HasOne(r => r.Reader).WithMany(u => u.Reservations)
            .HasForeignKey(r => r.IdReader).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Reservations>()
            .HasOne(r => r.Book).WithMany()
            .HasForeignKey(r => r.IdBook).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Reservations>()
            .HasOne(r => r.BookCopy).WithMany()
            .HasForeignKey(r => r.IdBookCopy).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Reservations>()
            .HasOne(r => r.Loan).WithMany()
            .HasForeignKey(r => r.IdLoan).OnDelete(DeleteBehavior.Restrict);
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

        // ==================== FINES ====================
        modelBuilder.Entity<Fines>().ToTable("Fines");
        modelBuilder.Entity<Fines>().HasKey(f => f.Id);
        modelBuilder.Entity<Fines>().Property(f => f.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Fines>()
            .Property(f => f.ViolationType).HasConversion<string>().IsRequired();
        modelBuilder.Entity<Fines>()
            .Property(f => f.Status).HasConversion<string>().IsRequired();
        modelBuilder.Entity<Fines>()
            .Property(f => f.AmountVnd).HasColumnType("INTEGER");
        modelBuilder.Entity<Fines>()
            .HasOne(f => f.LoanDetail)
            .WithMany(d => d.Fines)
            .HasForeignKey(f => f.IdLoanDetail)
            .IsRequired(false)                       // ⭐ IdLoanDetail nullable
            .OnDelete(DeleteBehavior.Restrict);
        // 9. Fines
        modelBuilder.Entity<Fines>(entity =>
        {
            entity.ToTable("Fines");
            entity.HasKey(f => f.Id);

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
        // ==================== FINE PAYMENTS ====================
        modelBuilder.Entity<FinePayments>().ToTable("FinePayments");
        modelBuilder.Entity<FinePayments>().HasKey(p => p.Id);
        modelBuilder.Entity<FinePayments>().Property(p => p.PaidAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<FinePayments>().Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<FinePayments>().Property(p => p.AmountVnd).HasColumnType("INTEGER");
        modelBuilder.Entity<FinePayments>()
            .Property(p => p.PaymentMethod).HasConversion<string>().IsRequired();
        modelBuilder.Entity<FinePayments>()
            .HasOne(p => p.Fine).WithMany(f => f.FinePayments)
            .HasForeignKey(p => p.IdFine).OnDelete(DeleteBehavior.Restrict);

        // ==================== REMINDERS ====================
        modelBuilder.Entity<Reminders>().ToTable("Reminders");
        modelBuilder.Entity<Reminders>().HasKey(r => r.Id);
        modelBuilder.Entity<Reminders>().Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Reminders>().Property(r => r.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Reminders>().Property(r => r.Channel).HasConversion<string>().IsRequired();
        modelBuilder.Entity<Reminders>().Property(r => r.Status).HasConversion<string>().IsRequired();
        modelBuilder.Entity<Reminders>()
            .HasOne(r => r.LoanDetail).WithMany(ld => ld.Reminders)
            .HasForeignKey(r => r.IdLoanDetail).OnDelete(DeleteBehavior.Cascade);

        // ==================== FINE AUDIT LOGS ====================
        modelBuilder.Entity<FineAuditLogs>().ToTable("FineAuditLogs");
        modelBuilder.Entity<FineAuditLogs>().HasKey(e => e.Id);
        modelBuilder.Entity<FineAuditLogs>().Property(e => e.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<FineAuditLogs>().Property(e => e.Action).HasConversion<string>().IsRequired();
        modelBuilder.Entity<FineAuditLogs>().Property(e => e.BeforeAmount).HasColumnType("INTEGER").HasDefaultValue(0);
        modelBuilder.Entity<FineAuditLogs>().Property(e => e.AfterAmount).HasColumnType("INTEGER").HasDefaultValue(0);
        modelBuilder.Entity<FineAuditLogs>().Property(e => e.At).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<FineAuditLogs>()
            .HasOne(e => e.Fine).WithMany(f => f.AuditLogs)
            .HasForeignKey(e => e.IdFine).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<FineAuditLogs>()
            .HasOne(e => e.Actor).WithMany()
            .HasForeignKey(e => e.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
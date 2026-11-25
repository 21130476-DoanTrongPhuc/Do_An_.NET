using LibraryManagement.Enums;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext() { }
    
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

    public DbSet<Users> Users { get; set; }
    public DbSet<Categories> BookCategories { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookCopies> BookCopies { get; set; }
    public DbSet<BookCopyHistory> BookCopyHistories { get; set; }
    public DbSet<ReaderCards> ReaderCards { get; set; }
    public DbSet<Loans> Loans { get; set; }
    public DbSet<LoanDetails> LoanDetails { get; set; }
    public DbSet<Reservations> Reservations { get; set; }
    public DbSet<Fines> Fines { get; set; }
    public DbSet<FinePayments> FinePayments { get; set; }
    public DbSet<FineAuditLogs> FineAuditLogs { get; set; }
    public DbSet<Reminders> Reminders { get; set; }
    public DbSet<FavoriteBook> FavoriteBooks { get; set; }

     protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================================================
        // 1. USERS
        // =========================================================
        modelBuilder.Entity<Users>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.UserCode).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.Username).HasColumnType("TEXT");
            e.Property(x => x.PasswordHash).HasColumnType("TEXT");
            e.Property(x => x.FullName).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.Phone).HasColumnType("TEXT");
            e.Property(x => x.Email).HasColumnType("TEXT");
            e.Property(x => x.Address).HasColumnType("TEXT");

            e.Property(x => x.UserType)
             .IsRequired()
             .HasConversion<string>()
             .HasColumnType("TEXT");

            e.Property(x => x.Status)
             .IsRequired()
             .HasConversion<string>()
             .HasDefaultValue(UserStatus.Active)
             .HasColumnType("TEXT");

            e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => x.UserCode).IsUnique();
            e.HasIndex(x => x.Username).IsUnique();
            e.HasIndex(x => new { x.UserType, x.Status })
             .HasDatabaseName("IX_Users_UserType_Status");

            // Filtered unique index (1 active librarian) — EF không khai báo filter được
            // → sẽ thêm qua raw SQL trong migration (xem mục 2)
        });

        // =========================================================
        // 2. BOOK CATEGORIES
        // =========================================================
        modelBuilder.Entity<Categories>(e =>
        {
            e.ToTable("BookCategories");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.CategoryCode).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.CategoryName).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.MaxLoanDays).IsRequired().HasDefaultValue(14);
            e.Property(x => x.MaxRenewals).IsRequired().HasDefaultValue(1);
            e.Property(x => x.Status)
             .IsRequired()
             .HasConversion<string>()
             .HasDefaultValue(BookCategoryStatus.Active)
             .HasColumnType("TEXT");

            e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => x.CategoryCode).IsUnique();
            e.HasIndex(x => x.IdParent).HasDatabaseName("IX_BookCategories_IdParent");

            // Self-reference
            e.HasOne(x => x.ParentCategory)
             .WithMany(x => x.Children)
             .HasForeignKey(x => x.IdParent)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // 3. BOOKS
        // =========================================================
        modelBuilder.Entity<Book>(e =>
        {
            e.ToTable("Books");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.BookCode).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.Isbn).HasColumnType("TEXT");
            e.Property(x => x.Title).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.Author).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.Publisher).HasColumnType("TEXT");

            // ⚠️ SQL: INTEGER, Entity: string? → phải đổi entity thành int?
            e.Property(x => x.PublishedYear).HasColumnType("INTEGER");

            e.Property(x => x.Language).HasColumnType("TEXT");
            e.Property(x => x.CoverImagePath).HasColumnType("TEXT");
            e.Property(x => x.Status).IsRequired().HasDefaultValue("Active").HasColumnType("TEXT");
            e.Property(x => x.Description).HasColumnType("TEXT");
            e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => x.BookCode).IsUnique();
            e.HasIndex(x => x.Isbn).IsUnique();
            e.HasIndex(x => x.IdCategory).HasDatabaseName("IX_Books_IdCategory");
            e.HasIndex(x => x.Title).HasDatabaseName("IX_Books_Title");

            e.HasOne(x => x.Category)
             .WithMany(c => c.Books)
             .HasForeignKey(x => x.IdCategory)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // 4. BOOK COPIES
        // =========================================================
        modelBuilder.Entity<BookCopies>(e =>
        {
            e.ToTable("BookCopies");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.CopyCode).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.ShelfLocation).HasColumnType("TEXT");

            // ⚠️ SQL: TEXT DEFAULT CURRENT_TIMESTAMP, Entity: string
            e.Property(x => x.ReceivedAt)
             .IsRequired()
             .HasDefaultValueSql("CURRENT_TIMESTAMP")
             .HasColumnType("TEXT");

            e.Property(x => x.PurchasePriceVnd)
             .IsRequired()
             .HasDefaultValue(0)
             .HasColumnType("INTEGER");

            e.Property(x => x.PhysicalCondition)
             .IsRequired()
             .HasConversion<string>()
             .HasDefaultValue(BookCondition.Good)
             .HasColumnType("TEXT");

            e.Property(x => x.CirculationStatus)
             .IsRequired()
             .HasConversion<string>()
             .HasDefaultValue(CirculationStatus.Available)
             .HasColumnType("TEXT");

            // ⚠️ SQL cột tên "Note", Entity property tên "Notes" → phải map
            e.Property(x => x.Notes).HasColumnName("Note").HasColumnType("TEXT");
            e.Property(x => x.Source).HasColumnType("TEXT");

            e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(x => x.CopyCode).IsUnique();
            e.HasIndex(x => x.IdBook).HasDatabaseName("IX_BookCopies_IdBook");
            e.HasIndex(x => x.CirculationStatus).HasDatabaseName("IX_BookCopies_Status");

            // Composite unique (Id, IdBook) — dùng cho composite FK ở Reservations
            e.HasAlternateKey(x => new { x.Id, x.IdBook })
             .HasName("AK_BookCopies_Id_IdBook");

            e.HasOne(x => x.Book)
             .WithMany(b => b.Copies)
             .HasForeignKey(x => x.IdBook)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // 5. BOOK COPY HISTORY
        // ⚠️ SQL có 2 bảng: "BookCopyHistory" và "BookCopyHistories"
        //    Entity: BookCopyHistory → map về "BookCopyHistories"
        // =========================================================
        modelBuilder.Entity<BookCopyHistory>(e =>
        {
            e.ToTable("BookCopyHistories");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.ChangedAt)
             .IsRequired()
             .HasDefaultValueSql("CURRENT_TIMESTAMP")
             .HasColumnType("TEXT");

            e.Property(x => x.ActionType).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.OldStatus).HasConversion<string?>().HasColumnType("TEXT");
            e.Property(x => x.NewStatus).HasConversion<string?>().HasColumnType("TEXT");
            e.Property(x => x.OldCondition).HasConversion<string?>().HasColumnType("TEXT");
            e.Property(x => x.NewCondition).HasConversion<string?>().HasColumnType("TEXT");
            e.Property(x => x.OldLocation).HasColumnType("TEXT");
            e.Property(x => x.NewLocation).HasColumnType("TEXT");
            e.Property(x => x.Note).HasColumnType("TEXT");

            e.HasIndex(x => x.IdBookCopy).HasDatabaseName("IX_BookCopyHistory_IdBookCopy");

            e.HasOne(x => x.BookCopy)
             .WithMany()                          // BookCopies KHÔNG có nav ngược
             .HasForeignKey(x => x.IdBookCopy)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================================================
        // 6. READER CARDS
        // =========================================================
        modelBuilder.Entity<ReaderCards>(e =>
        {
            e.ToTable("ReaderCards");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.CardCode).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.IssuedAt).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.ExpiresAt).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.Status)
             .IsRequired()
             .HasConversion<string>()
             .HasDefaultValue(ReaderCardStatus.Active)
             .HasColumnType("TEXT");
            e.Property(x => x.Note).HasColumnType("TEXT");

            e.HasIndex(x => x.CardCode).IsUnique();
            e.HasIndex(x => x.IdReader).HasDatabaseName("IX_ReaderCards_IdReader");

            e.HasOne(x => x.Reader)
             .WithMany(u => u.ReaderCards)
             .HasForeignKey(x => x.IdReader)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // 7. LOANS
        // =========================================================
        modelBuilder.Entity<Loans>(e =>
        {
            e.ToTable("Loans");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.LoanCode).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.BorrowedAt)
             .IsRequired()
             .HasDefaultValueSql("CURRENT_TIMESTAMP")
             .HasColumnType("TEXT");
            e.Property(x => x.Note).HasColumnType("TEXT");

            e.HasIndex(x => x.LoanCode).IsUnique();
            e.HasIndex(x => x.IdReader).HasDatabaseName("IX_Loans_IdReader");
            e.HasIndex(x => x.BorrowedAt).HasDatabaseName("IX_Loans_BorrowedAt");

            e.HasOne(x => x.Reader)
             .WithMany(u => u.BorrowedLoans)
             .HasForeignKey(x => x.IdReader)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // 8. LOAN DETAILS
        // =========================================================
        modelBuilder.Entity<LoanDetails>(e =>
        {
            e.ToTable("LoanDetails");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.DueAt).IsRequired().HasColumnType("TEXT");
            e.Property(x => x.ReturnedAt).HasColumnType("TEXT");

            e.Property(x => x.PhysicalConditionBefore)
             .IsRequired()
             .HasConversion<string>()
             .HasColumnType("TEXT");

            e.Property(x => x.PhysicalConditionAfter)
             .HasConversion<string?>()
             .HasColumnType("TEXT");

            e.Property(x => x.RenewalCount).IsRequired().HasDefaultValue(0);
            e.Property(x => x.Note).HasColumnType("TEXT");

            e.HasIndex(x => x.IdLoan).HasDatabaseName("IX_LoanDetails_IdLoan");
            e.HasIndex(x => x.DueAt).HasDatabaseName("IX_LoanDetails_DueAt");

            e.HasOne(x => x.Loan)
             .WithMany(l => l.LoanDetails)
             .HasForeignKey(x => x.IdLoan)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.BookCopy)
             .WithMany(c => c.LoanDetails)
             .HasForeignKey(x => x.IdBookCopy)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // 9. RESERVATIONS
        // =========================================================
        modelBuilder.Entity<Reservations>(e =>
        {
            e.ToTable("Reservations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.ReservationCode).IsRequired().HasColumnType("TEXT");

            e.Property(x => x.ReservedAt)
             .IsRequired()
             .HasDefaultValueSql("CURRENT_TIMESTAMP")
             .HasColumnType("TEXT");

            e.Property(x => x.ExpiresAt).HasColumnType("TEXT");
            e.Property(x => x.AssignedAt).HasColumnType("TEXT");
            e.Property(x => x.FulfilledAt).HasColumnType("TEXT");
            e.Property(x => x.CancelledAt).HasColumnType("TEXT");

            e.Property(x => x.Status)
             .IsRequired()
             .HasConversion<string>()
             .HasDefaultValue(ReservationStatus.Pending)
             .HasColumnType("TEXT");

            e.Property(x => x.Note).HasColumnType("TEXT");

            e.HasIndex(x => x.ReservationCode).IsUnique();
            e.HasIndex(x => x.IdReader).HasDatabaseName("IX_Reservations_IdReader");
            e.HasIndex(x => new { x.Status, x.ExpiresAt })
             .HasDatabaseName("IX_Reservations_Status_ExpiresAt");

            e.HasOne(x => x.Reader)
             .WithMany(u => u.Reservations)
             .HasForeignKey(x => x.IdReader)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Book)
             .WithMany(b => b.Reservations)
             .HasForeignKey(x => x.IdBook)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Loan)
             .WithOne()
             .HasForeignKey<Reservations>(x => x.IdLoan)
             .OnDelete(DeleteBehavior.Restrict);

            // ⚠️ Composite FK: (IdBookCopy, IdBook) → BookCopies(Id, IdBook)
            e.HasOne(x => x.BookCopy)
             .WithMany(c => c.Reservations)
             .HasForeignKey(x => new { x.IdBookCopy, x.IdBook })
             .HasPrincipalKey(c => new { c.Id, c.IdBook })
             .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // 10. FINES
        // =========================================================
        modelBuilder.Entity<Fines>(e =>
        {
            e.ToTable("Fines");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.FineCode).IsRequired().HasColumnType("TEXT");

            // ⚠️ SQL: INTEGER (nullable), Entity: int (non-nullable) → đổi thành int?
            e.Property(x => x.IdLoanDetail).HasColumnType("INTEGER");

            e.Property(x => x.CreatedAt)
             .IsRequired()
             .HasDefaultValueSql("CURRENT_TIMESTAMP")
             .HasColumnType("TEXT");

            e.Property(x => x.ViolationType)
             .IsRequired()
             .HasConversion<string>()
             .HasColumnType("TEXT");

            e.Property(x => x.AmountVnd).IsRequired().HasColumnType("INTEGER");
            e.Property(x => x.Description).HasColumnType("TEXT");

            e.Property(x => x.Status)
             .IsRequired()
             .HasConversion<string>()
             .HasDefaultValue(FineStatus.Unpaid)
             .HasColumnType("TEXT");

            e.Property(x => x.CancelledAt).HasColumnType("TEXT");
            e.Property(x => x.CancelReason).HasColumnType("TEXT");
            e.Property(x => x.UpdatedAt).HasColumnType("TEXT");

            e.HasIndex(x => x.FineCode).IsUnique();
            e.HasIndex(x => x.IdLoanDetail).HasDatabaseName("IX_Fines_IdLoanDetail");
            e.HasIndex(x => x.Status).HasDatabaseName("IX_Fines_Status");
            e.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_Fines_CreatedAt");

            e.HasOne(x => x.LoanDetail)
             .WithMany(l => l.Fines)
             .HasForeignKey(x => x.IdLoanDetail)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // 11. FINE PAYMENTS
        // =========================================================
        modelBuilder.Entity<FinePayments>(e =>
        {
            e.ToTable("FinePayments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            // ⚠️ SQL NOT NULL nhưng Entity cho nullable → nên đổi thành string (non-null)
            e.Property(x => x.PaymentCode).IsRequired().HasColumnType("TEXT");

            e.Property(x => x.PaidAt)
             .IsRequired()
             .HasDefaultValueSql("CURRENT_TIMESTAMP")
             .HasColumnType("TEXT");

            e.Property(x => x.AmountVnd).IsRequired().HasColumnType("INTEGER");

            e.Property(x => x.PaymentMethod)
             .IsRequired()
             .HasConversion<string>()
             .HasDefaultValue(PaymentMethod.Cash)
             .HasColumnType("TEXT");

            e.Property(x => x.Note).HasColumnType("TEXT");
            e.Property(x => x.CreatedAt)
             .IsRequired()
             .HasDefaultValueSql("CURRENT_TIMESTAMP")
             .HasColumnType("TEXT");

            e.HasIndex(x => x.PaymentCode).IsUnique();
            e.HasIndex(x => x.IdFine).HasDatabaseName("IX_FinePayments_IdFine");
            e.HasIndex(x => x.PaidAt).HasDatabaseName("IX_FinePayments_PaidAt");

            e.HasOne(x => x.Fine)
             .WithMany(f => f.FinePayments)
             .HasForeignKey(x => x.IdFine)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // 12. FINE AUDIT LOGS
        // =========================================================
        modelBuilder.Entity<FineAuditLogs>(e =>
        {
            e.ToTable("FineAuditLogs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.Action)
             .IsRequired()
             .HasConversion<string>()
             .HasColumnType("TEXT");

            e.Property(x => x.BeforeAmount).IsRequired().HasDefaultValue(0);
            e.Property(x => x.AfterAmount).IsRequired().HasDefaultValue(0);
            e.Property(x => x.Reason).HasColumnType("TEXT");

            e.Property(x => x.At)
             .IsRequired()
             .HasDefaultValueSql("CURRENT_TIMESTAMP")
             .HasColumnType("TEXT");

            e.HasIndex(x => x.IdFine).HasDatabaseName("IX_FineAuditLogs_IdFine");
            e.HasIndex(x => x.At).HasDatabaseName("IX_FineAuditLogs_At");

            e.HasOne(x => x.Fine)
             .WithMany(f => f.AuditLogs)
             .HasForeignKey(x => x.IdFine)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================================================
        // 13. REMINDERS
        // =========================================================
        modelBuilder.Entity<Reminders>(e =>
        {
            e.ToTable("Reminders");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.Channel)
             .IsRequired()
             .HasConversion<string>()
             .HasColumnType("TEXT");

            e.Property(x => x.Status)
             .IsRequired()
             .HasConversion<string>()
             .HasDefaultValue(ReminderStatus.Pending)
             .HasColumnType("TEXT");

            e.Property(x => x.SentAt).HasColumnType("TEXT");
            e.Property(x => x.Content).HasColumnType("TEXT");
            e.Property(x => x.ErrorMessage).HasColumnType("TEXT");

            e.Property(x => x.CreatedAt)
             .IsRequired()
             .HasDefaultValueSql("CURRENT_TIMESTAMP")
             .HasColumnType("TEXT");
            e.Property(x => x.UpdatedAt)
             .IsRequired()
             .HasDefaultValueSql("CURRENT_TIMESTAMP")
             .HasColumnType("TEXT");

            e.HasIndex(x => new { x.IdLoanDetail, x.CreatedAt })
             .HasDatabaseName("IX_Reminders_LoanDetail_CreatedAt");

            e.HasIndex(x => x.Status).HasDatabaseName("IX_Reminders_Status");

            e.HasOne(x => x.LoanDetail)
             .WithMany(l => l.Reminders)
             .HasForeignKey(x => x.IdLoanDetail)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================================================
        // 14. FAVORITE BOOKS (composite PK)
        // =========================================================
        modelBuilder.Entity<FavoriteBook>(e =>
        {
            e.ToTable("FavoriteBooks");
            e.HasKey(x => new { x.ReaderId, x.BookId });
            
            e.Property(x => x.ReaderId)
             .HasColumnName("IdReader");

            e.Property(x => x.BookId)
             .HasColumnName("IdBook");
            // ⚠️ Entity dùng "AddedDate", SQL cột tên "AddedAt" → map
            e.Property(x => x.AddedDate)
             .HasColumnName("AddedAt")
             .HasDefaultValueSql("CURRENT_TIMESTAMP")
             .HasColumnType("TEXT");

            e.HasOne(x => x.Reader)
             .WithMany()
             .HasForeignKey(x => x.ReaderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Book)
             .WithMany()
             .HasForeignKey(x => x.BookId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
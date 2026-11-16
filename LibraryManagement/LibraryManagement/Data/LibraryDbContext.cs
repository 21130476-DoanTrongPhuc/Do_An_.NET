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

        modelBuilder.Entity<Book>().ToTable("Books");
        modelBuilder.Entity<Categories>().ToTable("BookCategories");
        modelBuilder.Entity<BookCopies>().ToTable("BookCopies");
        modelBuilder.Entity<Users>().ToTable("Users");
        modelBuilder.Entity<ReaderCards>().ToTable("ReaderCards");
        
        // 1. Cấu hình Khóa chính (Primary Key) cho bảng Loans
        modelBuilder.Entity<Loans>().HasKey(l => l.Id);

        // 2. Map tên cột (nếu tên property khác tên cột trong DB)
        modelBuilder.Entity<Loans>().Property(l => l.BorrowDate).HasColumnName("BorrowedAt");

        // 3. Cấu hình mối quan hệ Độc giả (Reader)
        modelBuilder.Entity<Loans>()
            .HasOne(l => l.Reader)
            .WithMany(u => u.BorrowedLoans) // Nối với ICollection BorrowedLoans
            .HasForeignKey(l => l.IdReader)
            .OnDelete(DeleteBehavior.Restrict);

        // 4. Cấu hình mối quan hệ Thủ thư (Librarian)
        modelBuilder.Entity<Loans>()
            .HasOne(l => l.Librarian)
            .WithMany(u => u.ProcessedLoans) // Nối với ICollection ProcessedLoans
            .HasForeignKey(l => l.IdLibrarian)
            .OnDelete(DeleteBehavior.Restrict);
                
        // (Tùy chọn) Cấu hình tên bảng nếu cần
        modelBuilder.Entity<Loans>().ToTable("Loans");
        
        modelBuilder.Entity<LoanDetails>().ToTable("LoanDetails");
        
        // 1. Quan hệ với Độc giả (Reader)
        modelBuilder.Entity<Reservations>()
            .HasOne(r => r.Reader)
            .WithMany(u => u.Reservations) // Nối với ICollection Reservations (dành cho Reader) ở class Users
            .HasForeignKey(r => r.IdReader)
            .OnDelete(DeleteBehavior.Restrict);

// 2. Quan hệ với Thủ thư xử lý (Librarian)
        modelBuilder.Entity<Reservations>()
            .HasOne(r => r.Librarian)
            .WithMany(u => u.ProcessedReservations) // Nối với ICollection ProcessedReservations ở class Users
            .HasForeignKey(r => r.IdLibrarian)
            .OnDelete(DeleteBehavior.Restrict);

// 3. Quan hệ với Sách (Book)
        modelBuilder.Entity<Reservations>()
            .HasOne(r => r.Book)
            .WithMany() // Nếu class Books không có ICollection<Reservations> thì để trống
            .HasForeignKey(r => r.IdBook)
            .OnDelete(DeleteBehavior.Restrict);

// 4. Quan hệ với Bản sao Sách (BookCopy)
        modelBuilder.Entity<Reservations>()
            .HasOne(r => r.BookCopy)
            .WithMany() 
            .HasForeignKey(r => r.IdBookCopy)
            .OnDelete(DeleteBehavior.Restrict);

// 5. Quan hệ với Phiếu mượn (Loan)
        modelBuilder.Entity<Reservations>()
            .HasOne(r => r.Loan)
            .WithMany() 
            .HasForeignKey(r => r.IdLoan)
            .OnDelete(DeleteBehavior.Restrict);

// 6. Cấu hình tên cột (nếu có sai lệch giữa C# và SQL)
        modelBuilder.Entity<Reservations>().Property(r => r.ExpiresAt).HasColumnName("ExpiresAt");
        modelBuilder.Entity<Reservations>().Property(r => r.Note).HasColumnName("Note");
        
        modelBuilder.Entity<Fines>().ToTable("Fines");
        modelBuilder.Entity<FinePayments>().ToTable("FinePayments");
    }
}
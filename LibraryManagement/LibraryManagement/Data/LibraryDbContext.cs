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
    public DbSet<Readers> Readers { get; set; }
    public DbSet<ReaderCards> ReaderCards { get; set; }
    public DbSet<Librarians> Librarians { get; set; }
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
        modelBuilder.Entity<Readers>().ToTable("Readers");
        modelBuilder.Entity<ReaderCards>().ToTable("ReaderCards");
        modelBuilder.Entity<Librarians>().ToTable("Librarians");
        modelBuilder.Entity<Loans>().ToTable("Loans");
        modelBuilder.Entity<LoanDetails>().ToTable("LoanDetails");
        modelBuilder.Entity<Reservations>().ToTable("Reservations");
        modelBuilder.Entity<Fines>().ToTable("Fines");
        modelBuilder.Entity<FinePayments>().ToTable("FinePayments");
    }
}
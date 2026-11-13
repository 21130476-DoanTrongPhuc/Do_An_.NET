using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories;

public class LoanRepository(IDbContextFactory<LibraryDbContext> contextFactory)
{
    public async Task<List<Loans>> GetAllAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Loans
            .AsNoTracking()
            .Include(l => l.Reader)
            .Include(l => l.Librarian)
            .Include(l => l.LoanDetails)
            .ToListAsync();
    }

    public async Task<Loans?> GetByIdAsync(int id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Loans
            .Include(l => l.Reader)
            .Include(l => l.Librarian)
            .Include(l => l.LoanDetails)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Loans> AddAsync(Loans loan)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        context.Loans.Add(loan);
        await context.SaveChangesAsync();
        return loan;
    } // <-- Dấu ngoặc này bị thiếu khiến 2 hàm dưới bị nuốt vào bên trong

    public async Task UpdateAsync(Loans loan)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        context.Loans.Update(loan);
        await context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var loan = await context.Loans.FindAsync(id);
        if (loan == null) return false;

        context.Loans.Remove(loan);
        await context.SaveChangesAsync();
        return true;
    }
}
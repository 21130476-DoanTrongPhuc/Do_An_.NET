using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services;

public class ManageOverdueServiceImpl
{
    private readonly IDbContextFactory<LibraryDbContext> _contextFactory;
    private readonly ManageOverdueBookRepository  _bookRepository;

    public ManageOverdueServiceImpl(IDbContextFactory<LibraryDbContext> contextFactory, ManageOverdueBookRepository bookRepository)
    {
        _contextFactory = contextFactory;
        _bookRepository = bookRepository;
    }
    
     public async Task<List<ManageOverdueBookInfo>> GetManageOverdueBooks(ManageOverdueBookFilter filter)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var repo = _bookRepository.GetManageOverdueBooks(context, filter);
        return await repo;
    }

    public async  Task AddReminderToUser(int idLoanDetail, ReminderChannel channel, string content)
    {
        await using var context = await  _contextFactory.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();
        
        try
        {
            var reminderRepo = _bookRepository;

            await reminderRepo.AddReminderToUser(context, idLoanDetail, channel, content);
            
            await context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (Exception e)
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
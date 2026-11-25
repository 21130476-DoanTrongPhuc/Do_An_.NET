using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Filters;
using LibraryManagement.Repositories;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services;

public class ReturnedBookServiceImpl 
{
    
    private readonly IDbContextFactory<LibraryDbContext> _contextFactory;
    private readonly ReturnedBookRepository _bookRepository;
    private readonly FineRepository _fineRepository;

    public ReturnedBookServiceImpl(IDbContextFactory<LibraryDbContext> contextFactory, ReturnedBookRepository bookRepository, FineRepository fineRepository)
    {
        _contextFactory = contextFactory;
        _bookRepository = bookRepository;
        _fineRepository = fineRepository;
    }


    public async Task<ReturnedBookReaderInfor?> GetReaderInforAsync(string userCode)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var repo = _bookRepository;
        return await repo.GetReaderInforAsync(context, userCode);
    }

    public async Task<List<ReturnedBookInfor>?> GetReturneedBookInfor(ReturnedBookFilter filter)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var repo = _bookRepository;
        return await repo.GetReturnedBookInfor(context, filter);
    }

    public async Task ReturnAsync(int id, ReturnedBookAddRequest request)
    {
        await using var context = await  _contextFactory.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();
        
        try
        {
            var returnBookRepo = _bookRepository;
            
            await returnBookRepo.UpdateAsync(context, id, request);
            
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
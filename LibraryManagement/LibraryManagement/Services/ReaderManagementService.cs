using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Filters;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services;

public class ReaderManagementService
{
    private readonly IDbContextFactory<LibraryDbContext> _contextFactory;
    private readonly ReaderManagementRepository _managementRepository;

    public ReaderManagementService(IDbContextFactory<LibraryDbContext> contextFactory, ReaderManagementRepository managementRepository)
    {
        _contextFactory = contextFactory;
        _managementRepository = managementRepository;
    }

    public async Task<ReaderListSummaryResponse> GetReaders(ReaderFilter filter)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await _managementRepository.GetReaders(context, filter);
    }

    public async Task<ReaderDetailResponse?> GetReaderDetail(int readerId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await _managementRepository.GetReaderDetail(context, readerId);
    }

    public async Task UpdateReaderInfo(ReaderUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Họ tên độc giả không được để trống.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();

        try
        {
            await _managementRepository.UpdateReaderInfo(context, request);
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<DateTime> ExtendOrIssueCard(int readerId, int months)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();

        try
        {
            var newExpireDate = await _managementRepository.ExtendOrIssueCard(context, readerId, months);
            await tx.CommitAsync();
            return newExpireDate;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<(string FullName, bool IsNowActive)> ToggleLockAccountAsync(int readerId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();

        try
        {
            var result = await _managementRepository.ToggleLockAccount(context, readerId);
            await tx.CommitAsync();
            return result;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
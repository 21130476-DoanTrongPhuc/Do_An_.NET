using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Filters;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services;

public class LibraryCardService
{
    private readonly IDbContextFactory<LibraryDbContext> _contextFactory;
    private readonly LibraryCardRepository _cardRepository;

    public LibraryCardService(
        IDbContextFactory<LibraryDbContext> contextFactory,
        LibraryCardRepository cardRepository)
    {
        _contextFactory = contextFactory;
        _cardRepository = cardRepository;
    }

    public async Task<LibraryCardListSummaryResponse> GetCards(LibraryCardFilter filter)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await _cardRepository.GetCards(context, filter);
    }

    public async Task<LibraryCardDetailResponse?> GetCardDetail(int readerId, int? cardId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await _cardRepository.GetCardDetail(context, readerId, cardId);
    }

    public async Task<string> IssueOrUpdateCard(LibraryCardSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CardCode))
            throw new ArgumentException("Mã thẻ thư viện không được để trống.");

        if (request.ExpiresAt.Date <= request.IssuedAt.Date)
            throw new ArgumentException("Ngày hết hạn thẻ phải lớn hơn ngày cấp thẻ.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();

        try
        {
            var code = await _cardRepository.IssueOrUpdateCard(context, request);
            await tx.CommitAsync();
            return code;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<(string CardCode, DateTime NewExpiresAt)> ExtendCard(int cardId, int months)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();

        try
        {
            var result = await _cardRepository.ExtendCard(context, cardId, months);
            await tx.CommitAsync();
            return result;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<string> ToggleLockCard(int cardId, string? note)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();

        try
        {
            var code = await _cardRepository.ToggleLockCard(context, cardId, note);
            await tx.CommitAsync();
            return code;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
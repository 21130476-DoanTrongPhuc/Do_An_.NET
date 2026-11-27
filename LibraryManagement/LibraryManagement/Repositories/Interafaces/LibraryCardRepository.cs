using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Filters;

namespace LibraryManagement.Repositories.Interafaces;

public interface LibraryCardRepository
{
    Task<LibraryCardListSummaryResponse> GetCards(LibraryDbContext context, LibraryCardFilter filter);
    Task<LibraryCardDetailResponse?> GetCardDetail(LibraryDbContext context, int readerId, int? cardId);
    Task<string> IssueOrUpdateCard(LibraryDbContext context, LibraryCardSaveRequest request);
    Task<(string CardCode, DateTime NewExpiresAt)> ExtendCard(LibraryDbContext context, int cardId, int months);
    Task<string> ToggleLockCard(LibraryDbContext context, int cardId, string? note);
}
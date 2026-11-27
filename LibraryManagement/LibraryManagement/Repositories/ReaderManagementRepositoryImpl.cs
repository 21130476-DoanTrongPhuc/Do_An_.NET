using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Filters;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories;

public class ReaderManagementRepositoryImpl : ReaderManagementRepository
{
    public async Task<ReaderListSummaryResponse> GetReaders(LibraryDbContext context, ReaderFilter filter)
    {
        var today = DateTime.Today;
        string keyword = (filter.Keyword ?? string.Empty).Trim().ToLower();

        var rawReaders = await context.Users
            .Where(u => u.UserType == UserType.Reader)
            .Select(u => new
            {
                u.Id,
                u.UserCode,
                u.FullName,
                u.Email,
                u.Phone,
                u.Status,
                LatestCard = context.ReaderCards
                    .Where(rc => rc.IdReader == u.Id)
                    .OrderByDescending(rc => rc.ExpiresAt)
                    .Select(rc => new { rc.CardCode, rc.ExpiresAt, rc.Status })
                    .FirstOrDefault(),
                ActiveLoans = context.LoanDetails
                    .Count(ld => ld.Loan.IdReader == u.Id && ld.ReturnedAt == null),
                OverdueLoans = context.LoanDetails
                    .Count(ld => ld.Loan.IdReader == u.Id && ld.ReturnedAt == null && ld.DueAt.Date < today)
            })
            .OrderByDescending(x => x.Id)
            .AsNoTracking()
            .ToListAsync();

        int totalReaders = rawReaders.Count;
        int activeCount = rawReaders.Count(r => r.Status == UserStatus.Active);

        var filtered = rawReaders.Where(r =>
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                bool match = (r.UserCode?.ToLower().Contains(keyword) ?? false)
                          || (r.FullName?.ToLower().Contains(keyword) ?? false)
                          || (r.Email?.ToLower().Contains(keyword) ?? false)
                          || (r.Phone?.ToLower().Contains(keyword) ?? false);
                if (!match) return false;
            }

            DateTime? exp = r.LatestCard?.ExpiresAt.Date;
            return filter.StatusFilterMode switch
            {
                1 => r.Status == UserStatus.Active,
                2 => r.Status == UserStatus.Active && exp != null && exp >= today && exp <= today.AddDays(30),
                3 => exp == null || exp < today,
                4 => r.Status != UserStatus.Active,
                _ => true
            };
        }).ToList();

        var items = filtered.Select(r =>
        {
            DateTime? exp = r.LatestCard?.ExpiresAt.Date;
            string statusText =
                r.Status != UserStatus.Active  ? "Bị khóa" :
                exp == null || exp < today     ? "Hết hạn thẻ" :
                (exp.Value - today).Days <= 30 ? "Sắp hết hạn" :
                                                 "Hoạt động";

            string contact = !string.IsNullOrWhiteSpace(r.Phone) ? r.Phone : (r.Email ?? "--");
            string borrowText = r.OverdueLoans > 0
                ? $"{r.ActiveLoans} ({r.OverdueLoans} trễ)"
                : $"{r.ActiveLoans} cuốn";

            return new ReaderListItemResponse
            {
                Id = r.Id,
                UserCode = r.UserCode,
                FullName = r.FullName,
                Contact = contact,
                ExpireDateText = exp?.ToString("dd/MM/yyyy") ?? "Chưa cấp",
                BorrowText = borrowText,
                StatusText = statusText
            };
        }).ToList();

        return new ReaderListSummaryResponse
        {
            TotalReaders = totalReaders,
            ActiveReaders = activeCount,
            Items = items
        };
    }

    public async Task<ReaderDetailResponse?> GetReaderDetail(LibraryDbContext context, int readerId)
    {
        var today = DateTime.Today;

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == readerId);

        if (user == null) return null;

        var card = await context.ReaderCards
            .Where(rc => rc.IdReader == readerId)
            .OrderByDescending(rc => rc.ExpiresAt)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        int borrowing = await context.LoanDetails
            .CountAsync(ld => ld.Loan.IdReader == readerId && ld.ReturnedAt == null);
        int overdue = await context.LoanDetails
            .CountAsync(ld => ld.Loan.IdReader == readerId && ld.ReturnedAt == null && ld.DueAt.Date < today);
        int totalHistory = await context.LoanDetails
            .CountAsync(ld => ld.Loan.IdReader == readerId);

        return new ReaderDetailResponse
        {
            Id = user.Id,
            UserCode = user.UserCode,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            IsActive = user.Status == UserStatus.Active,
            HasCard = card != null,
            CardCode = card?.CardCode ?? string.Empty,
            CardExpiresAt = card?.ExpiresAt,
            IsCardExpired = card != null && card.ExpiresAt.Date < today,
            BorrowingCount = borrowing,
            OverdueCount = overdue,
            TotalHistoryCount = totalHistory
        };
    }

    public async Task UpdateReaderInfo(LibraryDbContext context, ReaderUpdateRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == request.Id)
                   ?? throw new InvalidOperationException("Không tìm thấy thông tin độc giả.");

        user.FullName = request.FullName.Trim();
        user.Email = request.Email?.Trim();
        user.Phone = request.Phone?.Trim();

        await context.SaveChangesAsync();
    }

    public async Task<DateTime> ExtendOrIssueCard(LibraryDbContext context, int readerId, int months)
    {
        var card = await context.ReaderCards
            .Where(rc => rc.IdReader == readerId)
            .OrderByDescending(rc => rc.ExpiresAt)
            .FirstOrDefaultAsync();

        if (card == null)
        {
            card = new ReaderCards
            {
                IdReader = readerId,
                CardCode = $"CARD-{DateTime.Now:yyMMdd}-{readerId:D4}",
                IssuedAt = DateTime.Now,
                ExpiresAt = DateTime.Today.AddMonths(months),
                Status = ReaderCardStatus.Active
            };
            context.ReaderCards.Add(card);
        }
        else
        {
            var baseDate = card.ExpiresAt.Date < DateTime.Today ? DateTime.Today : card.ExpiresAt.Date;
            card.ExpiresAt = baseDate.AddMonths(months);
            card.Status = ReaderCardStatus.Active;
        }

        await context.SaveChangesAsync();
        return card.ExpiresAt;
    }

    public async Task<(string FullName, bool IsNowActive)> ToggleLockAccount(LibraryDbContext context, int readerId)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == readerId)
                   ?? throw new InvalidOperationException("Không tìm thấy thông tin độc giả.");

        bool currentlyActive = user.Status == UserStatus.Active;
        user.Status = currentlyActive ? UserStatus.Inactive : UserStatus.Active;

        await context.SaveChangesAsync();
        return (user.FullName, !currentlyActive);
    }
}
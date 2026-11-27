using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Filters;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories;

public class LibraryCardRepositoryImpl : LibraryCardRepository
{
    public async Task<LibraryCardListSummaryResponse> GetCards(LibraryDbContext context, LibraryCardFilter filter)
    {
        var today = DateTime.Today;
        string kw = (filter.Keyword ?? string.Empty).Trim().ToLower();

        var readerCards = await context.Users
            .Where(u => u.UserType == UserType.Reader)
            .Select(u => new
            {
                ReaderId = u.Id,
                u.UserCode,
                u.FullName,
                u.Phone,
                LatestCard = context.ReaderCards
                    .Where(c => c.IdReader == u.Id)
                    .OrderByDescending(c => c.Status == ReaderCardStatus.Active)
                    .ThenByDescending(c => c.ExpiresAt)
                    .Select(c => new
                    {
                        c.Id,
                        c.CardCode,
                        c.IssuedAt,
                        c.ExpiresAt,
                        c.Status
                    })
                    .FirstOrDefault()
            })
            .OrderByDescending(x => x.LatestCard != null ? x.LatestCard.Id : 0)
            .ThenByDescending(x => x.ReaderId)
            .AsNoTracking()
            .ToListAsync();

        int activeCount = 0, expiringCount = 0, expiredOrLockedCount = 0, missingCount = 0;

        var viewItems = readerCards.Select(x =>
        {
            string displayStatus;
            if (x.LatestCard == null)
            {
                displayStatus = "Chưa cấp thẻ";
                missingCount++;
            }
            else if (x.LatestCard.Status == ReaderCardStatus.Locked)
            {
                displayStatus = "Đang bị khóa";
                expiredOrLockedCount++;
            }
            else if (x.LatestCard.Status == ReaderCardStatus.Expired || x.LatestCard.ExpiresAt.Date < today)
            {
                displayStatus = "Đã hết hạn";
                expiredOrLockedCount++;
            }
            else if ((x.LatestCard.ExpiresAt.Date - today).Days <= 30)
            {
                displayStatus = "Sắp hết hạn";
                expiringCount++;
                activeCount++;
            }
            else
            {
                displayStatus = "Đang hoạt động";
                activeCount++;
            }

            return new
            {
                x.ReaderId,
                CardId = x.LatestCard?.Id,
                CardCode = x.LatestCard?.CardCode ?? "—",
                x.UserCode,
                x.FullName,
                Phone = x.Phone ?? "—",
                IssuedAt = x.LatestCard?.IssuedAt,
                ExpiresAt = x.LatestCard?.ExpiresAt,
                DisplayStatus = displayStatus
            };
        }).ToList();

        var filtered = viewItems.Where(item =>
        {
            if (!string.IsNullOrEmpty(kw))
            {
                bool match = item.CardCode.ToLower().Contains(kw)
                          || item.UserCode.ToLower().Contains(kw)
                          || item.FullName.ToLower().Contains(kw)
                          || item.Phone.ToLower().Contains(kw);
                if (!match) return false;
            }

            return filter.StatusFilterIndex switch
            {
                1 => item.DisplayStatus is "Đang hoạt động" or "Sắp hết hạn",
                2 => item.DisplayStatus == "Sắp hết hạn",
                3 => item.DisplayStatus == "Đã hết hạn",
                4 => item.DisplayStatus == "Đang bị khóa",
                5 => item.DisplayStatus == "Chưa cấp thẻ",
                _ => true
            };
        }).ToList();

        var items = filtered.Select(item => new LibraryCardListItemResponse
        {
            ReaderId = item.ReaderId,
            CardId = item.CardId,
            CardCode = item.CardCode,
            ReaderDisplay = $"{item.FullName} ({item.UserCode})",
            Phone = item.Phone,
            IssuedAtText = item.IssuedAt?.ToString("dd/MM/yyyy") ?? "—",
            ExpiresAtText = item.ExpiresAt?.ToString("dd/MM/yyyy") ?? "—",
            DisplayStatus = item.DisplayStatus
        }).ToList();

        return new LibraryCardListSummaryResponse
        {
            ActiveCount = activeCount,
            ExpiringCount = expiringCount,
            ExpiredOrLockedCount = expiredOrLockedCount,
            MissingCount = missingCount,
            Items = items
        };
    }

    public async Task<LibraryCardDetailResponse?> GetCardDetail(LibraryDbContext context, int readerId, int? cardId)
    {
        var today = DateTime.Today;

        var reader = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == readerId);
        if (reader == null) return null;

        var card = cardId.HasValue
            ? await context.ReaderCards.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cardId.Value)
            : await context.ReaderCards.AsNoTracking()
                .Where(c => c.IdReader == readerId)
                .OrderByDescending(c => c.ExpiresAt)
                .FirstOrDefaultAsync();

        if (card == null)
        {
            string suggestedCode = await GenerateNextCardCodeAsync(context);
            return new LibraryCardDetailResponse
            {
                ReaderId = reader.Id,
                ReaderFullName = reader.FullName,
                ReaderCode = reader.UserCode,
                HasCard = false,
                CardCode = suggestedCode,
                IssuedAt = today,
                ExpiresAt = today.AddYears(1)
            };
        }

        bool isLocked = card.Status == ReaderCardStatus.Locked;
        bool isExpired = card.Status == ReaderCardStatus.Expired || card.ExpiresAt.Date < today;

        return new LibraryCardDetailResponse
        {
            ReaderId = reader.Id,
            ReaderFullName = reader.FullName,
            ReaderCode = reader.UserCode,
            CardId = card.Id,
            HasCard = true,
            CardCode = card.CardCode,
            IssuedAt = card.IssuedAt,
            ExpiresAt = card.ExpiresAt,
            Note = card.Note ?? string.Empty,
            IsLocked = isLocked,
            IsExpired = isExpired
        };
    }

    public async Task<string> IssueOrUpdateCard(LibraryDbContext context, LibraryCardSaveRequest request)
    {
        var today = DateTime.Today;
        string code = request.CardCode.Trim().ToUpper();
        string? note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        if (request.CardId == null)
        {
            bool exists = await context.ReaderCards.AnyAsync(c => c.CardCode.ToUpper() == code);
            if (exists)
                throw new InvalidOperationException($"Mã thẻ '{code}' đã tồn tại trên hệ thống. Vui lòng chọn mã khác.");

            var newCard = new ReaderCards
            {
                IdReader = request.ReaderId,
                CardCode = code,
                IssuedAt = request.IssuedAt.Date,
                ExpiresAt = request.ExpiresAt.Date,
                Status = request.ExpiresAt.Date < today ? ReaderCardStatus.Expired : ReaderCardStatus.Active,
                Note = note
            };

            context.ReaderCards.Add(newCard);
            await context.SaveChangesAsync();
            return code;
        }
        else
        {
            var card = await context.ReaderCards.FirstOrDefaultAsync(c => c.Id == request.CardId.Value)
                       ?? throw new InvalidOperationException("Không tìm thấy thẻ thư viện cần cập nhật.");

            card.IssuedAt = request.IssuedAt.Date;
            card.ExpiresAt = request.ExpiresAt.Date;
            card.Note = note;

            if (card.Status != ReaderCardStatus.Locked)
            {
                card.Status = card.ExpiresAt.Date < today ? ReaderCardStatus.Expired : ReaderCardStatus.Active;
            }

            await context.SaveChangesAsync();
            return card.CardCode;
        }
    }

    public async Task<(string CardCode, DateTime NewExpiresAt)> ExtendCard(LibraryDbContext context, int cardId, int months)
    {
        var card = await context.ReaderCards.FirstOrDefaultAsync(c => c.Id == cardId)
                   ?? throw new InvalidOperationException("Không tìm thấy thẻ thư viện.");

        if (card.Status == ReaderCardStatus.Locked)
            throw new InvalidOperationException("Thẻ đang bị KHÓA. Vui lòng mở khóa thẻ trước khi gia hạn.");

        DateTime baseDate = card.ExpiresAt.Date < DateTime.Today ? DateTime.Today : card.ExpiresAt.Date;
        card.ExpiresAt = baseDate.AddMonths(months);
        card.Status = ReaderCardStatus.Active;

        await context.SaveChangesAsync();
        return (card.CardCode, card.ExpiresAt);
    }

    public async Task<string> ToggleLockCard(LibraryDbContext context, int cardId, string? note)
    {
        var card = await context.ReaderCards.FirstOrDefaultAsync(c => c.Id == cardId)
                   ?? throw new InvalidOperationException("Không tìm thấy thẻ thư viện.");

        bool currentlyLocked = card.Status == ReaderCardStatus.Locked;

        if (currentlyLocked)
        {
            card.Status = card.ExpiresAt.Date < DateTime.Today ? ReaderCardStatus.Expired : ReaderCardStatus.Active;
        }
        else
        {
            card.Status = ReaderCardStatus.Locked;
        }

        if (!string.IsNullOrWhiteSpace(note))
            card.Note = note.Trim();

        await context.SaveChangesAsync();
        return card.CardCode;
    }

    private static async Task<string> GenerateNextCardCodeAsync(LibraryDbContext context)
    {
        int nextId = (await context.ReaderCards.MaxAsync(c => (int?)c.Id) ?? 0) + 1;
        string code = $"RC{nextId:D5}";
        while (await context.ReaderCards.AnyAsync(c => c.CardCode == code))
        {
            nextId++;
            code = $"RC{nextId:D5}";
        }
        return code;
    }
}
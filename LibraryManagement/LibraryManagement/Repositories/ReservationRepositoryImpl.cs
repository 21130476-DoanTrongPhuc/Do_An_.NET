using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Filters;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories;

public class ReservationRepositoryImpl : ReservationRepository
{
    private static readonly CirculationStatus CopyReservedStatus = ResolveReservedCopyStatus();

    public async Task<ReservationListSummaryResponse> GetReservations(LibraryDbContext context, ReservationFilter filter)
    {
        var today = DateTime.Today;
        string keyword = (filter.Keyword ?? string.Empty).Trim().ToLower();

        var list = await context.Reservations
            .Include(r => r.Reader)
            .Include(r => r.Book)
            .Include(r => r.BookCopy)
            .Select(r => new
            {
                r.Id,
                r.ReservationCode,
                ReaderCode = r.Reader.UserCode,
                ReaderName = r.Reader.FullName,
                BookCode = r.Book.BookCode,
                BookTitle = r.Book.Title,
                r.ReservedAt,
                r.ExpiresAt,
                r.FulfilledAt,
                r.CancelledAt,
                r.Status,
                AssignedCopyCode = r.BookCopy != null ? r.BookCopy.CopyCode : null,
                AvailableCount = context.BookCopies.Count(c =>
                    c.IdBook == r.IdBook && c.CirculationStatus == CirculationStatus.Available)
            })
            .OrderBy(r => r.Status == ReservationStatus.Pending ? 0 : 1)
            .ThenBy(r => r.ReservedAt)
            .AsNoTracking()
            .ToListAsync();

        int pendingCount = list.Count(x => x.Status == ReservationStatus.Pending);
        int readyCount   = list.Count(x => x.Status == ReservationStatus.ReadyForPickup && x.FulfilledAt == null && x.CancelledAt == null);

        var filtered = list.Where(r =>
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                bool match = r.ReservationCode.ToLower().Contains(keyword)
                          || r.ReaderName.ToLower().Contains(keyword)
                          || r.ReaderCode.ToLower().Contains(keyword)
                          || r.BookTitle.ToLower().Contains(keyword)
                          || r.BookCode.ToLower().Contains(keyword);
                if (!match) return false;
            }

            return filter.StatusFilterIndex switch
            {
                1 => r.Status == ReservationStatus.Pending,
                2 => r.Status == ReservationStatus.ReadyForPickup,
                3 => r.Status == ReservationStatus.Completed || r.FulfilledAt != null,
                4 => r.Status == ReservationStatus.Cancelled || r.Status == ReservationStatus.Expired || r.CancelledAt != null,
                _ => true
            };
        }).ToList();

        var items = filtered.Select(r => new ReservationListItemResponse
        {
            Id = r.Id,
            ReservationCode = r.ReservationCode,
            ReaderDisplay = $"{r.ReaderName} ({r.ReaderCode})",
            BookTitle = r.BookTitle,
            ReservedAtText = r.ReservedAt.ToString("dd/MM/yyyy HH:mm"),
            AvailableText = r.AvailableCount > 0 ? $"{r.AvailableCount} cuốn" : "Hết trên kệ",
            AssignedCopyCode = r.AssignedCopyCode ?? "Chưa gán",
            StatusText = MapReservationStatusText(r.Status, r.ExpiresAt, r.FulfilledAt, r.CancelledAt, today)
        }).ToList();

        return new ReservationListSummaryResponse
        {
            PendingCount = pendingCount,
            ReadyCount = readyCount,
            Items = items
        };
    }

    public async Task<ReservationDetailResponse?> GetReservationDetail(LibraryDbContext context, int reservationId)
    {
        var res = await context.Reservations
            .Include(r => r.Reader)
            .Include(r => r.Book)
            .Include(r => r.BookCopy)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (res == null) return null;

        string contact = !string.IsNullOrWhiteSpace(res.Reader.Email)
            ? res.Reader.Email
            : (res.Reader.Phone ?? "Chưa có SĐT/Email");

        var copyOptions = new List<ReservationCopyOptionResponse>();

        if (res.BookCopy != null)
        {
            copyOptions.Add(new ReservationCopyOptionResponse
            {
                Id = res.BookCopy.Id,
                CopyCode = res.BookCopy.CopyCode,
                Display = $"{res.BookCopy.CopyCode} — Đã giữ cho độc giả (Kệ: {res.BookCopy.ShelfLocation ?? "N/A"})"
            });
        }
        else
        {
            copyOptions = await context.BookCopies
                .Where(c => c.IdBook == res.IdBook && c.CirculationStatus == CirculationStatus.Available)
                .OrderBy(c => c.CopyCode)
                .Select(c => new ReservationCopyOptionResponse
                {
                    Id = c.Id,
                    CopyCode = c.CopyCode,
                    Display = $"{c.CopyCode} — Kệ: {c.ShelfLocation ?? "Chưa xếp kệ"} ({c.PhysicalCondition})"
                })
                .AsNoTracking()
                .ToListAsync();
        }

        return new ReservationDetailResponse
        {
            Id = res.Id,
            ReservationCode = res.ReservationCode,
            ReaderFullName = res.Reader.FullName,
            ReaderCode = res.Reader.UserCode,
            ReaderContact = contact,
            BookCode = res.Book.BookCode,
            BookTitle = res.Book.Title,
            ReservedAt = res.ReservedAt,
            ExpiresAt = res.ExpiresAt,
            Note = res.Note,
            IsPending = res.Status == ReservationStatus.Pending,
            IsReadyForPickup = res.Status == ReservationStatus.ReadyForPickup,
            HasAssignedCopy = res.BookCopy != null,
            CopyOptions = copyOptions
        };
    }

    public async Task<ReservationApproveResultResponse> ApproveAndNotify(LibraryDbContext context, ReservationApproveRequest request)
    {
        var res = await context.Reservations
                      .Include(r => r.Reader)
                      .Include(r => r.Book)
                      .FirstOrDefaultAsync(r => r.Id == request.ReservationId)
                  ?? throw new InvalidOperationException("Không tìm thấy phiếu đặt trước.");

        var copy = await context.BookCopies.FirstOrDefaultAsync(c => c.Id == request.BookCopyId)
                   ?? throw new InvalidOperationException("Không tìm thấy bản sách vật lý.");

        if (copy.CirculationStatus != CirculationStatus.Available)
            throw new InvalidOperationException("Bản sách này vừa được người khác mượn hoặc giữ chỗ. Vui lòng làm mới danh sách.");

        var now = DateTime.Now;
        var oldCopyStatus = copy.CirculationStatus;
        var expiresAt = DateTime.Today.AddDays(request.HoldDays).AddHours(20);

        // 1. Cập nhật phiếu đặt trước
        res.IdBookCopy = copy.Id;
        res.AssignedAt = now;
        res.ExpiresAt  = expiresAt;
        res.Status     = ReservationStatus.ReadyForPickup;
        res.Note       = request.NotifyMessage.Trim();

        // 2. Cập nhật trạng thái bản sách sang giữ chỗ
        copy.CirculationStatus = CopyReservedStatus;
        copy.UpdatedAt = now;

        // 3. Ghi lịch sử biến động bản sách
        context.BookCopyHistories.Add(new BookCopyHistory
        {
            IdBookCopy   = copy.Id,
            ChangedAt    = now.ToString("yyyy-MM-dd HH:mm:ss"),
            ActionType   = $"Giữ đặt trước ({res.ReservationCode})",
            OldStatus    = oldCopyStatus,
            NewStatus    = CopyReservedStatus,
            OldCondition = copy.PhysicalCondition,
            NewCondition = copy.PhysicalCondition,
            Note         = $"Độc giả: {res.Reader.FullName} ({res.Reader.UserCode})"
        });

        await context.SaveChangesAsync();

        string targetContact = !string.IsNullOrWhiteSpace(res.Reader.Email)
            ? $"Email: {res.Reader.Email}"
            : (!string.IsNullOrWhiteSpace(res.Reader.Phone) ? $"SĐT: {res.Reader.Phone}" : "Hệ thống thông báo nội bộ");

        return new ReservationApproveResultResponse
        {
            ReservationCode = res.ReservationCode,
            CopyCode = copy.CopyCode,
            ExpiresAt = expiresAt,
            TargetContact = targetContact
        };
    }

    public async Task<string> ResendNotification(LibraryDbContext context, int reservationId, string notifyMessage)
    {
        var res = await context.Reservations
                      .Include(r => r.Reader)
                      .FirstOrDefaultAsync(r => r.Id == reservationId)
                  ?? throw new InvalidOperationException("Không tìm thấy phiếu đặt trước.");

        res.Note = notifyMessage.Trim();
        await context.SaveChangesAsync();

        return res.Reader.Email ?? res.Reader.Phone ?? res.Reader.FullName;
    }

    public async Task CancelReservation(LibraryDbContext context, int reservationId)
    {
        var res = await context.Reservations
                      .Include(r => r.BookCopy)
                      .FirstOrDefaultAsync(r => r.Id == reservationId)
                  ?? throw new InvalidOperationException("Không tìm thấy phiếu đặt trước.");

        var now = DateTime.Now;
        res.CancelledAt = now;
        res.Status = ReservationStatus.Cancelled;

        // Giải phóng bản sách vật lý nếu đã gán trước đó
        if (res.BookCopy != null && res.BookCopy.CirculationStatus == CopyReservedStatus)
        {
            var oldStatus = res.BookCopy.CirculationStatus;
            res.BookCopy.CirculationStatus = CirculationStatus.Available;
            res.BookCopy.UpdatedAt = now;

            context.BookCopyHistories.Add(new BookCopyHistory
            {
                IdBookCopy = res.BookCopy.Id,
                ChangedAt  = now.ToString("yyyy-MM-dd HH:mm:ss"),
                ActionType = "ReserveCancelled",
                OldStatus  = oldStatus,
                NewStatus  = CirculationStatus.Available,
                Note       = $"Hủy phiếu đặt trước {res.ReservationCode}, trả sách về kệ"
            });
        }

        await context.SaveChangesAsync();
    }

    private static CirculationStatus ResolveReservedCopyStatus()
    {
        string[] candidates = { "Reserved", "OnHold", "Held" };
        foreach (var name in candidates)
        {
            if (Enum.TryParse<CirculationStatus>(name, true, out var val))
                return val;
        }
        return CirculationStatus.Available;
    }

    private static string MapReservationStatusText(
        ReservationStatus status, DateTime? expiresAt, DateTime? fulfilledAt, DateTime? cancelledAt, DateTime today)
    {
        if (status == ReservationStatus.Completed || fulfilledAt != null) return "Đã nhận sách";
        if (status == ReservationStatus.Cancelled || cancelledAt != null) return "Đã hủy";
        if (status == ReservationStatus.Expired || (expiresAt != null && expiresAt.Value.Date < today)) return "Hết hạn giữ";
        if (status == ReservationStatus.ReadyForPickup) return "Sẵn sàng lấy";
        return "Chờ duyệt";
    }
}
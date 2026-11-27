using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Filters;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services;

public class ReservationService
{
    private readonly IDbContextFactory<LibraryDbContext> _contextFactory;
    private readonly ReservationRepository _reservationRepository;

    public ReservationService(
        IDbContextFactory<LibraryDbContext> contextFactory,
        ReservationRepository reservationRepository)
    {
        _contextFactory = contextFactory;
        _reservationRepository = reservationRepository;
    }

    public async Task<ReservationListSummaryResponse> GetReservations(ReservationFilter filter)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await _reservationRepository.GetReservations(context, filter);
    }

    public async Task<ReservationDetailResponse?> GetReservationDetail(int reservationId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await _reservationRepository.GetReservationDetail(context, reservationId);
    }

    public string BuildNotificationTemplate(string readerName, string bookTitle, string copyCode, int holdDays)
    {
        DateTime holdUntil = DateTime.Today.AddDays(holdDays);
        return
            $"Xin chào {readerName},\r\n" +
            $"Cuốn sách \"{bookTitle}\" (Mã bản sách: {copyCode}) bạn đặt trước tại Thư viện LIBRA đã sẵn sàng.\r\n" +
            $"Vui lòng đến quầy thủ thư nhận sách trước ngày {holdUntil:dd/MM/yyyy}. Sau thời hạn này, hệ thống sẽ tự động hủy giữ chỗ.";
    }

    public async Task<ReservationApproveResultResponse> ApproveAndNotify(ReservationApproveRequest request)
    {
        if (request.BookCopyId <= 0)
            throw new ArgumentException("Vui lòng chọn bản sách sẵn sàng trên kệ để duyệt phiếu này.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();

        try
        {
            var result = await _reservationRepository.ApproveAndNotify(context, request);
            await tx.CommitAsync();
            return result;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<string> ResendNotification(int reservationId, string notifyMessage)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();

        try
        {
            var contact = await _reservationRepository.ResendNotification(context, reservationId, notifyMessage);
            await tx.CommitAsync();
            return contact;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task CancelReservation(int reservationId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();

        try
        {
            await _reservationRepository.CancelReservation(context, reservationId);
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
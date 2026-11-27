using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Filters;

namespace LibraryManagement.Repositories.Interafaces;

public interface ReservationRepository
{
    Task<ReservationListSummaryResponse> GetReservations(LibraryDbContext context, ReservationFilter filter);
    Task<ReservationDetailResponse?> GetReservationDetail(LibraryDbContext context, int reservationId);
    Task<ReservationApproveResultResponse> ApproveAndNotify(LibraryDbContext context, ReservationApproveRequest request);
    Task<string> ResendNotification(LibraryDbContext context, int reservationId, string notifyMessage);
    Task CancelReservation(LibraryDbContext context, int reservationId);
}
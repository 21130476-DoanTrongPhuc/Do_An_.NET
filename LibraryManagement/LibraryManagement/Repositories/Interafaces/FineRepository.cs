using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Filters;

namespace LibraryManagement.Repositories.Interafaces;

public interface FineRepository
{
    Task Add(LibraryDbContext context, CreateFineManagementRequest request);

    Task Update(LibraryDbContext context, UpdateFineManagementRequest request);

    Task AdjustFineAmount(LibraryDbContext context, int fineId, int fineAmount, string note);

    Task Delete(LibraryDbContext context, int fineId, string note);

    Task<List<FineManagementInfo>> GetFineManagementInfo(
        LibraryDbContext context,
        FineManagementFilter filter);

    Task<FineDetailInfo?> GetFineDetail(LibraryDbContext context, int fineId);
}
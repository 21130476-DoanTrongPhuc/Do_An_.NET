using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Filters;
using LibraryManagement.Models;

namespace LibraryManagement.Repositories.Interafaces;

public interface FineRepository
{
    Task Add(LibraryDbContext context, CreateFineManagementRequest request);
    Task Update(LibraryDbContext context,  int FineId);
    Task AdjustFineAmount(LibraryDbContext context, int FineId, double FineAmount, string note);
    Task Delete(LibraryDbContext context, int FineId, string note);
    Task<List<FineManagementInfo>> GetFineManagementInfo(LibraryDbContext context, FineManagementFilter filter);
}
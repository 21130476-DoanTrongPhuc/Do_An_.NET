using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Filters;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories;

public class FineRepositoryImpl : FineRepository
{
    public Task Add(LibraryDbContext context, CreateFineManagementRequest request)
    {
        throw new NotImplementedException();
    }

    public Task Update(LibraryDbContext context, int FineId)
    {
        throw new NotImplementedException();
    }

    public Task AdjustFineAmount(LibraryDbContext context, int FineId, double FineAmount, string note)
    {
        throw new NotImplementedException();
    }

    public Task Delete(LibraryDbContext context, int FineId, string note)
    {
        throw new NotImplementedException();
    }

    public Task Update(LibraryDbContext context, int FineId, double FineAmount)
    {
        throw new NotImplementedException();
    }

    public Task<List<FineManagementInfo>> GetFineManagementInfo(LibraryDbContext context, FineManagementFilter filter)
    {
        throw new NotImplementedException();
    }
}
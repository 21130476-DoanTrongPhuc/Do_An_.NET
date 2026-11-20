using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories;

public class FineRepositoryImpl : FineRepository
{
    private readonly IDbContextFactory<LibraryDbContext> _contextFactory;

    public FineRepositoryImpl(IDbContextFactory<LibraryDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Add(Fines fine)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Fines.Add(fine);
        context.SaveChanges();
    }
}
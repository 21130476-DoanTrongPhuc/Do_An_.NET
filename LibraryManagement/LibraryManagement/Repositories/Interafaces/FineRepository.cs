using LibraryManagement.Models;

namespace LibraryManagement.Repositories.Interafaces;

public interface FineRepository
{
    void Add(Fines fine);
}
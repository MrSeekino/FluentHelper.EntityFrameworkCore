using Microsoft.EntityFrameworkCore.Storage;

namespace FluentHelper.EntityFrameworkCore.InMemory.DbMemory
{
    internal interface IDbDataMemory
    {
        int SaveChanges();

        IDbContextTransaction BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction(bool noThrow = false);
    }

    interface IDbDataMemory<T> : IDbDataMemory
    {
        IQueryable<T> GetAll();

        void Add(T input);
        void AddRange(IEnumerable<T> inputList);

        void Remove(T input);
        void RemoveRange(IEnumerable<T> inputList);
    }
}

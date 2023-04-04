using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Linq;

namespace FluentHelper.EntityFrameworkCore.Moq
{
    interface IDataMocker
    {
        int SaveChanges();

        IDbContextTransaction BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction(bool noThrow = false);
    }

    interface IDataMocker<T> : IDataMocker
    {
        IQueryable<T> GetAll();

        void Add(T input);
        void AddRange(IEnumerable<T> inputList);

        void Remove(T input);
        void RemoveRange(IEnumerable<T> inputList);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentHelper.EntityFrameworkCore.Interfaces
{
    public interface IDbContext : IDisposable
    {
        DbContext GetContext();
        DbContext CreateNewContext();

        bool IsTransactionOpen();
        IDbContextTransaction BeginTransaction();

        void RollbackTransaction();
        void CommitTransaction();
        bool AreSavepointsSupported();
        void CreateSavepoint(string savePointName);
        void ReleaseSavepoint(string savePointName);
        void RollbackToSavepoint(string savePointName);

        IQueryable<T> Query<T>() where T : class;
        void Add<T>(T inputData) where T : class;
        void AddRange<T>(IEnumerable<T> inputData) where T : class;
        void Remove<T>(T inputData) where T : class;
        void RemoveRange<T>(IEnumerable<T> inputData) where T : class;

        int SaveChanges();

        T ExecuteOnDatabase<T>(Func<DatabaseFacade, T> funcToExecute);
    }
}

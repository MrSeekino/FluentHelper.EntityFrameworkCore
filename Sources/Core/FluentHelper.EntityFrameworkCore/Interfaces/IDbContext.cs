using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

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
        IQueryable<T> QueryNoTracking<T>() where T : class;

        void Add<T>(T inputData) where T : class;
        Task AddAsync<T>(T inputData) where T : class;

        void AddRange<T>(IEnumerable<T> inputData) where T : class;
        Task AddRangeAsync<T>(IEnumerable<T> inputData, CancellationToken cancellationToken = default) where T : class;

        void Remove<T>(T inputData) where T : class;
        void RemoveRange<T>(IEnumerable<T> inputData) where T : class;

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        T ExecuteOnDatabase<T>(Func<DatabaseFacade, T> funcToExecute);

        int ExecuteDelete<T>(Expression<Func<T, bool>> deletePredicate) where T : class;
        Task<int> ExecuteDeleteAsync<T>(Expression<Func<T, bool>> deletePredicate, CancellationToken cancellationToken = default) where T : class;

        int ExecuteUpdate<T>(Expression<Func<T, bool>> updatePredicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateSetPropertyCalls) where T : class;
        Task<int> ExecuteUpdateAsync<T>(Expression<Func<T, bool>> updatePredicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateSetPropertyCalls, , CancellationToken cancellationToken = default) where T : class;

        void ClearTracker();
    }
}

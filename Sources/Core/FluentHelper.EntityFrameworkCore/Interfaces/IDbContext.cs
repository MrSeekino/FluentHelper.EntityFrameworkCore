using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

        int ExecuteDelete<T>(Expression<Func<T, bool>> deletePredicate) where T : class;
        int ExecuteUpdate<T>(Expression<Func<T, bool>> updatePredicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateSetPropertyCalls) where T : class;

        void ClearTracker();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FluentHelper.EntityFrameworkCore.Interfaces
{
    public interface IDbContext : IDisposable
    {
        IDbContext WithConnectionString(string nameOrConnectionString);
        IDbContext WithLazyLoadingProxies();
        IDbContext WithMappingFromAssemblyOf<T>();

        IDbContext WithLogAction(Action<LogLevel, EventId, string> logAction, bool enableSensitiveDataLogging = false);

        DbContext GetContext();
        DbContext CreateNewContext();

        bool IsTransactionOpen();
        IDbContextTransaction BeginTransaction();
        IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel);
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
    }
}

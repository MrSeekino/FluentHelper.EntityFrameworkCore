using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FluentHelper.EntityFrameworkCore.Tests")]
namespace FluentHelper.EntityFrameworkCore.Common
{
    class EfDbContext : IDbContext
    {
        internal DbContext DbContext { get; set; }

        internal string ConnectionString { get; set; }

        internal Action<string> LogAction { get; set; }
        internal Func<EventId, LogLevel, bool> LogFilter { get; set; }

        internal bool EnableSensitiveDataLogging { get; set; }
        internal bool EnableLazyLoadingProxies { get; set; }

        internal Func<string, Action<string>, Func<EventId, LogLevel, bool>, bool, bool, EfDbModel> CreateDbContextBehaviour { get; set; }

        public EfDbContext() : this((cs, la, lf, sdl, llp) => { return new EfDbModel(cs, la, lf, sdl, llp); }) { }

        public EfDbContext(Func<string, Action<string>, Func<EventId, LogLevel, bool>, bool, bool, EfDbModel> createDbContextBehaviour)
        {
            DbContext = null;

            ConnectionString = null;

            LogAction = null;
            LogFilter = null;
            EnableSensitiveDataLogging = false;

            EnableLazyLoadingProxies = false;

            CreateDbContextBehaviour = createDbContextBehaviour;
        }

        internal void CreateDbContext()
        {
            DbContext?.Dispose();
            DbContext = CreateDbContextBehaviour(ConnectionString, LogAction, LogFilter, EnableSensitiveDataLogging, EnableLazyLoadingProxies);
        }

        public IDbContext SetConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }

        public IDbContext UseLazyLoadingProxies()
        {
            EnableLazyLoadingProxies = true;
            return this;
        }

        public IDbContext AddMappingFromAssemblyOf<T>()
        {
            var mappingAssembly = Assembly.GetAssembly(typeof(T));
            ((EfDbModel)GetContext()).AddMappingAssembly(mappingAssembly);

            return this;
        }

        public IDbContext SetLogAction(Action<string> logAction)
        {
            return SetLogAction(logAction, false, (e, l) => { return true; });
        }

        public IDbContext SetLogAction(Action<string> logAction, bool enableSensitiveDataLogging)
        {
            return SetLogAction(logAction, enableSensitiveDataLogging, (e, l) => { return true; });
        }

        public IDbContext SetLogAction(Action<string> logAction, Func<EventId, LogLevel, bool> logFilter)
        {
            return SetLogAction(logAction, false, logFilter);
        }

        public IDbContext SetLogAction(Action<string> logAction, bool enableSensitiveDataLogging, Func<EventId, LogLevel, bool> logFilter)
        {
            LogAction = logAction;
            LogFilter = logFilter;
            EnableSensitiveDataLogging = enableSensitiveDataLogging;
            return this;
        }

        public DbContext GetContext()
        {
            if (DbContext == null)
                CreateDbContext();

            return DbContext;
        }

        public DbContext CreateNewContext()
        {
            Dispose();

            return GetContext();
        }

        public bool IsTransactionOpen()
        {
            return GetContext().Database.CurrentTransaction != null;
        }

        public IDbContextTransaction BeginTransaction()
        {
            if (IsTransactionOpen())
                throw new Exception("A transaction is already open");

            return GetContext().Database.BeginTransaction();
        }

        public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (IsTransactionOpen())
                throw new Exception("A transaction is already open");

            return GetContext().Database.BeginTransaction(isolationLevel);
        }

        public void RollbackTransaction()
        {
            GetContext().Database.CurrentTransaction.Rollback();
        }

        public void CommitTransaction()
        {
            GetContext().Database.CurrentTransaction.Commit();
        }

        public bool AreSavepointsSupported()
        {
            return GetContext().Database.CurrentTransaction.SupportsSavepoints;
        }

        public void CreateSavepoint(string savePointName)
        {
            if (GetContext().Database.CurrentTransaction.SupportsSavepoints)
                GetContext().Database.CurrentTransaction.CreateSavepoint(savePointName);
        }

        public void ReleaseSavepoint(string savePointName)
        {
            if (GetContext().Database.CurrentTransaction.SupportsSavepoints)
                GetContext().Database.CurrentTransaction.ReleaseSavepoint(savePointName);
        }

        public void RollbackToSavepoint(string savePointName)
        {
            if (GetContext().Database.CurrentTransaction.SupportsSavepoints)
                GetContext().Database.CurrentTransaction.RollbackToSavepoint(savePointName);
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return GetContext().Set<T>().AsQueryable();
        }

        public void Add<T>(T inputData) where T : class
        {
            GetContext().Set<T>().Add(inputData);
        }

        public void AddRange<T>(IEnumerable<T> inputData) where T : class
        {
            GetContext().Set<T>().AddRange(inputData);
        }

        public void Remove<T>(T inputData) where T : class
        {
            GetContext().Set<T>().Remove(inputData);
        }

        public void RemoveRange<T>(IEnumerable<T> inputData) where T : class
        {
            GetContext().Set<T>().RemoveRange(inputData);
        }

        public int SaveChanges()
        {
            return GetContext().SaveChanges();
        }

        public void Dispose()
        {
            DbContext?.Dispose();
        }
    }
}

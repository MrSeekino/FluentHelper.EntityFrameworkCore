using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FluentHelper.EntityFrameworkCore.Tests")]
namespace FluentHelper.EntityFrameworkCore.Common
{
    class EfDbContext : IDbContext
    {
        internal DbContext? DbContext { get; set; }
        internal Action<DbContextOptionsBuilder>? DbProviderConfiguration { get; set; }
        internal Action<LogLevel, EventId, string>? LogAction { get; set; }

        internal bool EnableSensitiveDataLogging { get; set; }
        internal bool EnableLazyLoadingProxies { get; set; }

        internal Func<Action<DbContextOptionsBuilder>, Action<LogLevel, EventId, string>?, bool, bool, EfDbModel> CreateDbContextBehaviour { get; set; }

        public EfDbContext()
            : this((dbc, la, dl, llp) => { return new EfDbModel(dbc, la, dl, llp); }) { }

        public EfDbContext(Func<Action<DbContextOptionsBuilder>, Action<LogLevel, EventId, string>?, bool, bool, EfDbModel> createDbContextBehaviour)
        {
            DbContext = null;
            DbProviderConfiguration = null;
            LogAction = null;

            EnableSensitiveDataLogging = false;
            EnableLazyLoadingProxies = false;

            CreateDbContextBehaviour = createDbContextBehaviour;
        }

        internal void CreateDbContext()
        {
            DbContext?.Dispose();
            DbContext = CreateDbContextBehaviour(DbProviderConfiguration!, LogAction, EnableSensitiveDataLogging, EnableLazyLoadingProxies);
        }

        public IDbContext WithDbProviderConfiguration(Action<DbContextOptionsBuilder> dbProviderConfiguration)
        {
            DbProviderConfiguration = dbProviderConfiguration;
            return this;
        }

        public IDbContext WithLazyLoadingProxies()
        {
            EnableLazyLoadingProxies = true;
            return this;
        }

        public IDbContext WithMappingFromAssemblyOf<T>()
        {
            var mappingAssembly = Assembly.GetAssembly(typeof(T));
            if (mappingAssembly == null)
                throw new ArgumentException($"Could not find assembly with {typeof(T).Name}");

            ((EfDbModel)GetContext()).AddMappingAssembly(mappingAssembly);
            return this;
        }

        public IDbContext WithLogAction(Action<LogLevel, EventId, string> logAction, bool enableSensitiveDataLogging = false)
        {
            LogAction = logAction;
            EnableSensitiveDataLogging = enableSensitiveDataLogging;
            return this;
        }

        public DbContext GetContext()
        {
            if (DbContext == null)
                CreateDbContext();

            return DbContext!;
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

        //public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        //{
        //    if (IsTransactionOpen())
        //        throw new Exception("A transaction is already open");

        //    return GetContext().Database.BeginTransaction(isolationLevel);
        //}

        public void RollbackTransaction()
        {
            if (!IsTransactionOpen())
                return;

            GetContext().Database.CurrentTransaction!.Rollback();
        }

        public void CommitTransaction()
        {
            if (!IsTransactionOpen())
                return;

            GetContext().Database.CurrentTransaction!.Commit();
        }

        public bool AreSavepointsSupported()
        {
            if (!IsTransactionOpen())
                throw new Exception("Cannot check support for savepoints when there is not active transaction");

            return GetContext().Database.CurrentTransaction!.SupportsSavepoints;
        }

        public void CreateSavepoint(string savePointName)
        {
            if (!IsTransactionOpen())
                return;

            if (GetContext().Database.CurrentTransaction!.SupportsSavepoints)
                GetContext().Database.CurrentTransaction!.CreateSavepoint(savePointName);
        }

        public void ReleaseSavepoint(string savePointName)
        {
            if (!IsTransactionOpen())
                return;

            if (GetContext().Database.CurrentTransaction!.SupportsSavepoints)
                GetContext().Database.CurrentTransaction!.ReleaseSavepoint(savePointName);
        }

        public void RollbackToSavepoint(string savePointName)
        {
            if (!IsTransactionOpen())
                return;

            if (GetContext().Database.CurrentTransaction!.SupportsSavepoints)
                GetContext().Database.CurrentTransaction!.RollbackToSavepoint(savePointName);
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
            DbContext = null;
        }
    }
}

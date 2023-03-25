using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FluentHelper.EntityFrameworkCore.Tests")]
namespace FluentHelper.EntityFrameworkCore.Common
{
    internal class EfDbContext : IDbContext
    {
        internal DbContext? DbContext { get; set; }
        internal IDbConfig DbConfig { get; set; }
        internal IEnumerable<IDbMap> Mappings { get; set; }

        internal Func<IDbConfig, IEnumerable<IDbMap>, EfDbModel> CreateDbContextBehaviour { get; set; }

        public EfDbContext(IDbConfig dbConfig, IEnumerable<IDbMap> mappings)
            : this(dbConfig, mappings, (c, m) => new EfDbModel(c, m))
        { }

        public EfDbContext(IDbConfig dbConfig, IEnumerable<IDbMap> mappings, Func<IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour)
        {
            DbContext = null;

            DbConfig = dbConfig;
            Mappings = mappings;
            CreateDbContextBehaviour = createDbContextBehaviour;
        }

        internal void CreateDbContext()
        {
            DbContext?.Dispose();
            DbContext = CreateDbContextBehaviour(DbConfig, Mappings);
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

        public T ExecuteOnDatabase<T>(Func<DatabaseFacade, T> funcToExecute)
        {
            return funcToExecute(GetContext().Database);
        }
    }
}

﻿using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("FluentHelper.EntityFrameworkCore.Tests")]
namespace FluentHelper.EntityFrameworkCore.Common
{
    internal sealed class EfDbContext : IDbContext
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
                throw new InvalidOperationException("A transaction is already open");

            return GetContext().Database.BeginTransaction();
        }

        public void RollbackTransaction()
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("Cannot find an open transaction to rollback");

            GetContext().Database.CurrentTransaction!.Rollback();
        }

        public void CommitTransaction()
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("Cannot find an open transaction to commit");

            GetContext().Database.CurrentTransaction!.Commit();
        }

        public bool AreSavepointsSupported()
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("Cannot check support for savepoints when there is not active transaction");

            return GetContext().Database.CurrentTransaction!.SupportsSavepoints;
        }

        public void CreateSavepoint(string savePointName)
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("An open transaction is needed to create a savepoint");

            if (GetContext().Database.CurrentTransaction!.SupportsSavepoints)
                GetContext().Database.CurrentTransaction!.CreateSavepoint(savePointName);
        }

        public void ReleaseSavepoint(string savePointName)
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("An open transaction is needed to release a savepoint");

            if (GetContext().Database.CurrentTransaction!.SupportsSavepoints)
                GetContext().Database.CurrentTransaction!.ReleaseSavepoint(savePointName);
        }

        public void RollbackToSavepoint(string savePointName)
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("An open transaction is needed to rollback to a savepoint");

            if (GetContext().Database.CurrentTransaction!.SupportsSavepoints)
                GetContext().Database.CurrentTransaction!.RollbackToSavepoint(savePointName);
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return GetContext().Set<T>().AsQueryable();
        }

        public IQueryable<T> QueryNoTracking<T>() where T : class
        {
            return GetContext().Set<T>().AsQueryable().AsNoTracking();
        }

        public void Add<T>(T inputData) where T : class
        {
            GetContext().Set<T>().Add(inputData);
        }

        public async Task AddAsync<T>(T inputData, CancellationToken cancellationToken = default) where T : class
        {
            await GetContext().Set<T>().AddAsync(inputData, cancellationToken);
        }

        public void AddRange<T>(IEnumerable<T> inputData) where T : class
        {
            GetContext().Set<T>().AddRange(inputData);
        }

        public async Task AddRangeAsync<T>(IEnumerable<T> inputData, CancellationToken cancellationToken = default) where T : class
        {
            await GetContext().Set<T>().AddRangeAsync(inputData, cancellationToken);
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

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await GetContext().SaveChangesAsync(cancellationToken);
        }

        public T ExecuteOnDatabase<T>(Func<DatabaseFacade, T> funcToExecute)
        {
            return funcToExecute(GetContext().Database);
        }

        public int ExecuteDelete<T>(Expression<Func<T, bool>> deletePredicate) where T : class
        {
            return GetContext().Set<T>().Where(deletePredicate).ExecuteDelete();
        }

        public async Task<int> ExecuteDeleteAsync<T>(Expression<Func<T, bool>> deletePredicate, CancellationToken cancellationToken = default) where T : class
        {
            return await GetContext().Set<T>().Where(deletePredicate).ExecuteDeleteAsync();
        }

        public int ExecuteUpdate<T>(Expression<Func<T, bool>> updatePredicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateSetPropertyCalls) where T : class
        {
            return GetContext().Set<T>().Where(updatePredicate).ExecuteUpdate(updateSetPropertyCalls);
        }

        public async Task<int> ExecuteUpdateAsync<T>(Expression<Func<T, bool>> updatePredicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateSetPropertyCalls, , CancellationToken cancellationToken = default) where T : class
        {
            return await GetContext().Set<T>().Where(updatePredicate).ExecuteUpdateAsync(updateSetPropertyCalls, cancellationToken);
        }

        public void ClearTracker()
        {
            GetContext().ChangeTracker.Clear();
        }

        public void Dispose()
        {
            DbContext?.Dispose();
            DbContext = null;
        }
    }
}

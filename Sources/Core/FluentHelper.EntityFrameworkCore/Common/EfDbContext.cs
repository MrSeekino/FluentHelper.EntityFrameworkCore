using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
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
        private readonly ILoggerFactory _loggerFactory;
        private readonly IDbConfig _dbConfig;
        private readonly IEnumerable<IDbMap> _mappings;
        private readonly Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> _createDbContextBehaviour;

        private DbContext? _dbContext;

        public EfDbContext(ILoggerFactory loggerFactory, IDbConfig dbConfig, IEnumerable<IDbMap> mappings)
            : this(loggerFactory, dbConfig, mappings, (l, c, m) => new EfDbModel(l, c, m))
        { }

        public EfDbContext(ILoggerFactory loggerFactory, IDbConfig dbConfig, IEnumerable<IDbMap> mappings, Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour)
            : this(null, loggerFactory, dbConfig, mappings, createDbContextBehaviour)
        { }

        public EfDbContext(DbContext? dbContext, ILoggerFactory loggerFactory, IDbConfig dbConfig, IEnumerable<IDbMap> mappings, Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour)
        {
            _dbContext = dbContext;
            _loggerFactory = loggerFactory;
            _dbConfig = dbConfig;
            _mappings = mappings;
            _createDbContextBehaviour = createDbContextBehaviour;
        }

        private void CreateDbContext()
        {
            _dbContext?.Dispose();
            _dbContext = _createDbContextBehaviour(_loggerFactory, _dbConfig, _mappings);
        }

        public string? GetProviderName()
        {
            return GetContext().Database.ProviderName;
        }

        public DbContext GetContext()
        {
            if (_dbContext == null)
                CreateDbContext();

            return _dbContext!;
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

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (IsTransactionOpen())
                throw new InvalidOperationException("A transaction is already open");

            return await GetContext().Database.BeginTransactionAsync(cancellationToken);
        }

        public void RollbackTransaction()
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("Cannot find an open transaction to rollback");

            GetContext().Database.CurrentTransaction!.Rollback();
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("Cannot find an open transaction to rollback");

            await GetContext().Database.CurrentTransaction!.RollbackAsync(cancellationToken);
        }

        public void CommitTransaction()
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("Cannot find an open transaction to commit");

            GetContext().Database.CurrentTransaction!.Commit();
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("Cannot find an open transaction to commit");

            await GetContext().Database.CurrentTransaction!.CommitAsync(cancellationToken);
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

        public async Task CreateSavepointAsync(string savePointName, CancellationToken cancellationToken = default)
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("An open transaction is needed to create a savepoint");

            if (GetContext().Database.CurrentTransaction!.SupportsSavepoints)
                await GetContext().Database.CurrentTransaction!.CreateSavepointAsync(savePointName, cancellationToken);
        }

        public void ReleaseSavepoint(string savePointName)
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("An open transaction is needed to release a savepoint");

            if (GetContext().Database.CurrentTransaction!.SupportsSavepoints)
                GetContext().Database.CurrentTransaction!.ReleaseSavepoint(savePointName);
        }

        public async Task ReleaseSavepointAsync(string savePointName, CancellationToken cancellationToken = default)
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("An open transaction is needed to release a savepoint");

            if (GetContext().Database.CurrentTransaction!.SupportsSavepoints)
                await GetContext().Database.CurrentTransaction!.ReleaseSavepointAsync(savePointName, cancellationToken);
        }

        public void RollbackToSavepoint(string savePointName)
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("An open transaction is needed to rollback to a savepoint");

            if (GetContext().Database.CurrentTransaction!.SupportsSavepoints)
                GetContext().Database.CurrentTransaction!.RollbackToSavepoint(savePointName);
        }

        public async Task RollbackToSavepointAsync(string savePointName, CancellationToken cancellationToken = default)
        {
            if (!IsTransactionOpen())
                throw new InvalidOperationException("An open transaction is needed to rollback to a savepoint");

            if (GetContext().Database.CurrentTransaction!.SupportsSavepoints)
                await GetContext().Database.CurrentTransaction!.RollbackToSavepointAsync(savePointName, cancellationToken);
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return GetContext().Set<T>().AsQueryable();
        }

        public IQueryable<T> QueryNoTracking<T>() where T : class
        {
            return GetContext().Set<T>().AsQueryable().AsNoTracking();
        }

        public IQueryable<T> QueryRaw<T>(string sqlQuery, params object[] parameters) where T : class
        {
            return GetContext().Set<T>().FromSqlRaw(sqlQuery, parameters);
        }

        public void Add<T>(T inputData) where T : class
        {
            GetContext().Set<T>().Add(inputData);
        }

        public async Task AddAsync<T>(T inputData, CancellationToken cancellationToken = default) where T : class
        {
            await GetContext().Set<T>().AddAsync(inputData, cancellationToken).ConfigureAwait(false);
        }

        public void AddRange<T>(IEnumerable<T> inputData) where T : class
        {
            GetContext().Set<T>().AddRange(inputData);
        }

        public async Task AddRangeAsync<T>(IEnumerable<T> inputData, CancellationToken cancellationToken = default) where T : class
        {
            await GetContext().Set<T>().AddRangeAsync(inputData, cancellationToken).ConfigureAwait(false);
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
            return await GetContext().SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public void ExecuteOnDatabase(Action<DatabaseFacade> actionToExecute)
        {
            actionToExecute(GetContext().Database);
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
            return await GetContext().Set<T>().Where(deletePredicate).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        }

        public int ExecuteUpdate<T>(Expression<Func<T, bool>> updatePredicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateSetPropertyCalls) where T : class
        {
            return GetContext().Set<T>().Where(updatePredicate).ExecuteUpdate(updateSetPropertyCalls);
        }

        public async Task<int> ExecuteUpdateAsync<T>(Expression<Func<T, bool>> updatePredicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateSetPropertyCalls, CancellationToken cancellationToken = default) where T : class
        {
            return await GetContext().Set<T>().Where(updatePredicate).ExecuteUpdateAsync(updateSetPropertyCalls, cancellationToken).ConfigureAwait(false);
        }

        public int ExecuteSqlRaw(string sqlQuery, params object[] parameters)
        {
            return GetContext().Database.ExecuteSqlRaw(sqlQuery, parameters);
        }

        public async Task<int> ExecuteSqlRawAsync(string sqlQuery, IEnumerable<object> parameters, CancellationToken cancellationToken = default)
        {
            return await GetContext().Database.ExecuteSqlRawAsync(sqlQuery, parameters, cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> ExecuteSqlRawAsync(string sqlQuery, CancellationToken cancellationToken = default)
        {
            return await GetContext().Database.ExecuteSqlRawAsync(sqlQuery, cancellationToken).ConfigureAwait(false);
        }

        public void SetCommandTimeout(TimeSpan timeout)
        {
            GetContext().Database.SetCommandTimeout(timeout);
        }

        public void ClearTracker()
        {
            GetContext().ChangeTracker.Clear();
        }

        public bool CanConnect()
        {
            return GetContext().Database.CanConnect();
        }

        public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            return await GetContext().Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
        }

        public string? GetConnectionString()
        {
            return GetContext().Database.GetConnectionString();
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            _dbContext = null;
        }
    }
}

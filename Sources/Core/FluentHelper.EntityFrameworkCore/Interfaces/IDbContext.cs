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
        /// <summary>
        /// Get the name of the provider in use
        /// </summary>
        /// <returns></returns>
        string? GetProviderName();

        /// <summary>
        /// Get the current EF DbContext
        /// </summary>
        /// <returns></returns>
        DbContext GetContext();
        /// <summary>
        /// Dispose current EF DbContext, create a new one and returns it
        /// </summary>
        /// <returns></returns>
        DbContext CreateNewContext();

        /// <summary>
        /// Check if there is an open transaction on the DbContext
        /// </summary>
        /// <returns></returns>
        bool IsTransactionOpen();

        /// <summary>
        /// Initialize a new transaction. Throws if a transaction is already open
        /// </summary>
        /// <returns></returns>
        IDbContextTransaction BeginTransaction();
        /// <summary>
        /// Initialize a new transaction. Throws if a transaction is already open
        /// </summary>
        /// <returns></returns>
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rollback the current transaction. Throws if no transaction is found
        /// </summary>
        void RollbackTransaction();
        /// <summary>
        ///  Rollback the current transaction. Throws if no transaction is found
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commit the current transaction. Throws if no transaction is found
        /// </summary>
        void CommitTransaction();
        /// <summary>
        /// Commit the current transaction. Throws if no transaction is found
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if savepoints are supported in current context. Throws if a transaction is open
        /// </summary>
        /// <returns></returns>
        bool AreSavepointsSupported();

        /// <summary>
        /// Create a new savepoint. Throws if a transaction is not open
        /// </summary>
        /// <param name="savePointName">the name of the savepoint</param>
        void CreateSavepoint(string savePointName);
        /// <summary>
        /// Create a new savepoint. Throws if a transaction is not open
        /// </summary>
        /// <param name="savePointName">the name of the savepoint</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CreateSavepointAsync(string savePointName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Release the savepoint. Throws if a transaction is not open
        /// </summary>
        /// <param name="savePointName">the name of the savepoint</param>
        void ReleaseSavepoint(string savePointName);
        /// <summary>
        /// Release the savepoint. Throws if a transaction is not open
        /// </summary>
        /// <param name="savePointName">the name of the savepoint</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ReleaseSavepointAsync(string savePointName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rollback the status to the savepoint. Throws if a transaction is not open
        /// </summary>
        /// <param name="savePointName">the name of the savepoint</param>
        void RollbackToSavepoint(string savePointName);
        /// <summary>
        /// Rollback the status to the savepoint. Throws if a transaction is not open
        /// </summary>
        /// <param name="savePointName">the name of the savepoint</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RollbackToSavepointAsync(string savePointName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get an IQueryable for the specified T Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQueryable<T> Query<T>() where T : class;
        /// <summary>
        /// Get an IQueryable for the specified T Model with 'AsNoTracking' called
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQueryable<T> QueryNoTracking<T>() where T : class;
        /// <summary>
        /// Get an IQueryable for the specified T Model using the specified query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IQueryable<T> QueryRaw<T>(string sqlQuery, params object[] parameters) where T : class;

        /// <summary>
        /// Add an item to the current DbContext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputData">the item to be added</param>
        void Add<T>(T inputData) where T : class;
        /// <summary>
        /// Add an item to the current DbContext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputData">the item to be added</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task AddAsync<T>(T inputData, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Add a set of items to the current DbContext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputData">the set of items to be added</param>
        void AddRange<T>(IEnumerable<T> inputData) where T : class;
        /// <summary>
        /// Add a set of items to the current DbContext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputData">the set of items to be added</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task AddRangeAsync<T>(IEnumerable<T> inputData, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// remove an item from the current DbContext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputData">the item to be removed</param>
        void Remove<T>(T inputData) where T : class;
        /// <summary>
        /// remove a set of items from the current DbContext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputData">the items to be removed</param>
        void RemoveRange<T>(IEnumerable<T> inputData) where T : class;

        /// <summary>
        /// Save current tracked changes on the database
        /// </summary>
        /// <returns></returns>
        int SaveChanges();
        /// <summary>
        /// Save current tracked changes on the database
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Perform a specific actions against the database
        /// </summary>
        /// <param name="actionToExecute">The action to execute</param>
        void ExecuteOnDatabase(Action<DatabaseFacade> actionToExecute);
        /// <summary>
        /// Perform a specific actions against the database and return the result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="funcToExecute">The action to be executed</param>
        /// <returns></returns>
        T ExecuteOnDatabase<T>(Func<DatabaseFacade, T> funcToExecute);

        /// <summary>
        /// Execute a raw delete on the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deletePredicate">The predicate to be used in delete statement</param>
        /// <returns></returns>
        int ExecuteDelete<T>(Expression<Func<T, bool>> deletePredicate) where T : class;
        /// <summary>
        /// Execute a raw delete on the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deletePredicate">The predicate to be used in delete statement</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> ExecuteDeleteAsync<T>(Expression<Func<T, bool>> deletePredicate, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Execute a raw update on the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="updatePredicate">The predicate to be used in update statement</param>
        /// <param name="updateSetPropertyCalls"></param>
        /// <returns></returns>
        int ExecuteUpdate<T>(Expression<Func<T, bool>> updatePredicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateSetPropertyCalls) where T : class;
        /// <summary>
        /// Execute a raw update on the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="updatePredicate">The predicate to be used in update statement</param>
        /// <param name="updateSetPropertyCalls"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> ExecuteUpdateAsync<T>(Expression<Func<T, bool>> updatePredicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateSetPropertyCalls, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Execute a raw query against the database
        /// </summary>
        /// <param name="sqlQuery">the query template to be executed</param>
        /// <param name="parameters">the parameters for the query template</param>
        /// <returns></returns>
        int ExecuteSqlRaw(string sqlQuery, params object[] parameters);
        /// <summary>
        /// Execute a raw query against the database
        /// </summary>
        /// <param name="sqlQuery">the query template to be executed</param>
        /// <param name="parameters">the parameters for the query template</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> ExecuteSqlRawAsync(string sqlQuery, IEnumerable<object> parameters, CancellationToken cancellationToken = default);
        /// <summary>
        /// Execute a raw query against the database
        /// </summary>
        /// <param name="sqlQuery">the query template to be executed</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> ExecuteSqlRawAsync(string sqlQuery, CancellationToken cancellationToken = default);

        /// <summary>
        /// Set the timeout for the next commands
        /// </summary>
        /// <param name="timeout"></param>
        void SetCommandTimeout(TimeSpan timeout);
        /// <summary>
        /// Clear the current status of the EF DbContext tracker
        /// </summary>
        void ClearTracker();

        /// <summary>
        /// Check if the database can be reached
        /// </summary>
        /// <returns></returns>
        bool CanConnect();
        /// <summary>
        /// Check if the database can be reached
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the current connectionstring used by the DbContext
        /// </summary>
        /// <returns></returns>
        string? GetConnectionString();
    }
}

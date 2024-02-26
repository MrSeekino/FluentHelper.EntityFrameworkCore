using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace FluentHelper.EntityFrameworkCore.SqlServer
{
    public static class SqlProviderExtensions
    {
        /// <summary>
        /// Use MSSql as the provider
        /// </summary>
        /// <param name="dbContextBuilder"></param>
        /// <param name="connectionString">MSSql connectionstring</param>
        /// <param name="sqlServerOptionsAction">Specific settings to be applied</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static EfDbConfigBuilder WithSqlDbProvider(this EfDbConfigBuilder dbContextBuilder, string connectionString, Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            dbContextBuilder = dbContextBuilder.WithDbProvider(dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder = sqlServerOptionsAction != null ? dbContextOptionsBuilder.UseSqlServer(connectionString, sqlServerOptionsAction) : dbContextOptionsBuilder.UseSqlServer(connectionString);
            });

            return dbContextBuilder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IDbContextTransaction BeginTransaction(this IDbContext dbContext, IsolationLevel isolationLevel)
        {
            if (dbContext.IsTransactionOpen())
                throw new InvalidOperationException("A transaction is already open");

            return dbContext.ExecuteOnDatabase(db => db.BeginTransaction(isolationLevel));
        }

        /// <summary>
        /// Begin a transaction with specified isolation level
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="isolationLevel">The preferred isolation level</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<IDbContextTransaction> BeginTransactionAsync(this IDbContext dbContext, IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            if (dbContext.IsTransactionOpen())
                throw new InvalidOperationException("A transaction is already open");

            return await dbContext.ExecuteOnDatabase(db => db.BeginTransactionAsync(isolationLevel, cancellationToken));
        }
    }
}

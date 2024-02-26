using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace FluentHelper.EntityFrameworkCore.PostgreSql
{
    public static class PostgreSqlProviderExtensions
    {
        /// <summary>
        /// Use Postresql as the provider
        /// </summary>
        /// <param name="dbContextBuilder"></param>
        /// <param name="connectionString">Postgresql connectionstring</param>
        /// <param name="npgSqlOptionsAction">Specific settings to be applied</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static EfDbConfigBuilder WithPostgreSqlProvider(this EfDbConfigBuilder dbContextBuilder, string connectionString, Action<NpgsqlDbContextOptionsBuilder>? npgSqlOptionsAction = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            dbContextBuilder = dbContextBuilder.WithDbProvider(dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder = npgSqlOptionsAction != null ? dbContextOptionsBuilder.UseNpgsql(connectionString, npgSqlOptionsAction) : dbContextOptionsBuilder.UseNpgsql(connectionString);
            });

            return dbContextBuilder;
        }

        /// <summary>
        /// Begin a transaction with specified isolation level
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="isolationLevel">The preferred isolation level</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IDbContextTransaction BeginTransaction(this IDbContext dbContext, System.Data.IsolationLevel isolationLevel)
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
        public static async Task<IDbContextTransaction> BeginTransactionAsync(this IDbContext dbContext, System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            if (dbContext.IsTransactionOpen())
                throw new InvalidOperationException("A transaction is already open");

            return await dbContext.ExecuteOnDatabase(db => db.BeginTransactionAsync(isolationLevel, cancellationToken));
        }
    }
}

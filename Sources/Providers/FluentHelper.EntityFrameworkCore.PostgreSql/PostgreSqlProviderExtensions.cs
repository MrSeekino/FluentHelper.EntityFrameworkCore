using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace FluentHelper.EntityFrameworkCore.PostgreSql
{
    public static class PostgreSqlProviderExtensions
    {
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

        public static IDbContextTransaction BeginTransaction(this IDbContext dbContext, System.Data.IsolationLevel isolationLevel)
        {
            if (dbContext.IsTransactionOpen())
                throw new InvalidOperationException("A transaction is already open");

            return dbContext.ExecuteOnDatabase(db => db.BeginTransaction(isolationLevel));
        }

        public static async Task<IDbContextTransaction> BeginTransactionAsync(this IDbContext dbContext, System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            if (dbContext.IsTransactionOpen())
                throw new InvalidOperationException("A transaction is already open");

            return await dbContext.ExecuteOnDatabase(db => db.BeginTransactionAsync(isolationLevel, cancellationToken));
        }
    }
}

using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace FluentHelper.EntityFramworkCore.PostgreSQL
{
    public static class PostgreProviderExtensions
    {
        public static IDbContext WithSqlDbProvider(this IDbContext dbContext, string connectionString, Action<NpgsqlDbContextOptionsBuilder>? npgSqlOptionsAction = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            dbContext = dbContext.WithDbProviderConfiguration(dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder = npgSqlOptionsAction != null ? dbContextOptionsBuilder.UseNpgsql(connectionString, npgSqlOptionsAction) : dbContextOptionsBuilder.UseNpgsql(connectionString);
            });

            return dbContext;
        }

        public static IDbContextTransaction BeginTransaction(this IDbContext dbContext, System.Data.IsolationLevel isolationLevel)
        {
            if (dbContext.IsTransactionOpen())
                throw new Exception("A transaction is already open");

            return dbContext.ExecuteOnDatabase(db => db.BeginTransaction(isolationLevel));
        }
    }
}

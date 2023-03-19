using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace FluentHelper.EntityFrameworkCore.SqlServer
{
    public static class SqlProviderExtensions
    {
        public static IDbContext WithSqlDbProvider(this IDbContext dbContext, string connectionString, Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            dbContext = dbContext.WithDbProviderConfiguration(dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder = sqlServerOptionsAction != null ? dbContextOptionsBuilder.UseSqlServer(connectionString, sqlServerOptionsAction) : dbContextOptionsBuilder.UseSqlServer(connectionString);
            });

            return dbContext;
        }

        public static IDbContextTransaction BeginTransaction(this IDbContext dbContext, System.Data.IsolationLevel isolationLevel)
        {
            if (dbContext.IsTransactionOpen())
                throw new Exception("A transaction is already open");

            return dbContext.ExecuteOnDatabase((db) => db.BeginTransaction(isolationLevel));
        }
    }
}

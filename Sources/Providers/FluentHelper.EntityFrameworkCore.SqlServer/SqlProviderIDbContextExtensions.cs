using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FluentHelper.EntityFrameworkCore.SqlServer
{
    public static class SqlProviderIDbContextExtensions
    {
        public static IDbContext WithSqlDbProvider(this IDbContext dbContext, string connectionString, Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("Connection string cannot be null or empty");

            dbContext = dbContext.WithDbProviderConfiguration(dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder = sqlServerOptionsAction != null ? dbContextOptionsBuilder.UseSqlServer(connectionString, sqlServerOptionsAction) : dbContextOptionsBuilder.UseSqlServer(connectionString);
            });

            return dbContext;
        }
    }
}

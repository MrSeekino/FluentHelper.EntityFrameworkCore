using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FluentHelper.EntityFrameworkCore.Examples.Dao
{
    public static class IDbContextExtensions
    {
        public static IDbContext WithSqlDbProvider(this IDbContext dbContext, string connectionString)
        {
            dbContext = dbContext.WithDbProviderConfiguration(dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder.UseSqlServer(connectionString);
            });

            return dbContext;
        }
    }
}

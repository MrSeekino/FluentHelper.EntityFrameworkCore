using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FluentHelper.EntityFrameworkCore.Examples.Dao
{
    class SqlDbProviderConfiguration : IDbProviderConfiguration
    {
        string ConnectionString { get; set; }

        public SqlDbProviderConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public DbContextOptionsBuilder ConfigureDbProvider(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            dbContextOptionsBuilder.UseSqlServer(ConnectionString);

            return dbContextOptionsBuilder;
        }
    }

    public static class IDbContextExtensions
    {
        public static IDbContext WithSqlDbProvider(this IDbContext dbContext, string connectionString)
        {
            dbContext = dbContext.WithDbProviderConfiguration(new SqlDbProviderConfiguration(connectionString));
            return dbContext;
        }
    }
}

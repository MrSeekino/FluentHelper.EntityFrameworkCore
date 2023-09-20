using FluentHelper.EntityFrameworkCore.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FluentHelper.EntityFrameworkCore.InMemory
{
    public static class InMemoryProviderExtensions
    {
        public static EfDbConfigBuilder WithInMemoryProvider(this EfDbConfigBuilder dbContextBuilder, string databaseName, Action<InMemoryDbContextOptionsBuilder>? inMemoryOptionsAction = null)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentNullException(nameof(databaseName));

            dbContextBuilder.WithDbProvider(dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder = inMemoryOptionsAction != null ? dbContextOptionsBuilder.UseInMemoryDatabase(databaseName, inMemoryOptionsAction) : dbContextOptionsBuilder.UseInMemoryDatabase(databaseName);
            });

            return dbContextBuilder;
        }
    }
}
using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Examples.Mappings;
using FluentHelper.EntityFrameworkCore.InMemory;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace FluentHelper.EntityFrameworkCore.Examples.Tests
{
    public abstract class BaseRepositoriesTests<T> where T : class
    {
        protected T Repository { get; set; }

        public BaseRepositoriesTests()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddFluentDbContext(dbConfigBuilder =>
            {
                dbConfigBuilder
                    .WithDbConfiguration(c => c.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)))
                    .WithInMemoryProvider("TestDb")
                    .WithMappingFromAssemblyOf<TestDataMap>();
            });

            serviceCollection.AddSingleton<T, T>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            Repository = serviceProvider.GetRequiredService<T>();
        }
    }
}

using FluentHelper.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace FluentHelper.EntityFrameworkCore.Examples.Tests
{
    public abstract class BaseRepositoriesTests<TRepository> : IDisposable where TRepository : class
    {
        protected TRepository Repository { get; set; }

        public BaseRepositoriesTests()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddInMemoryContext();
            serviceCollection.AddSingleton<TRepository, TRepository>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            Repository = serviceProvider.GetRequiredService<TRepository>();
        }

        public void AddSupportTo<TEntity>(IEnumerable<TEntity>? initialData = null) where TEntity : class
        {
            InMemoryProviderExtensions.AddMemoryContextSupportTo(initialData);
        }

        public void Dispose()
        {
            InMemoryProviderExtensions.ClearMemoryContext();
        }
    }
}

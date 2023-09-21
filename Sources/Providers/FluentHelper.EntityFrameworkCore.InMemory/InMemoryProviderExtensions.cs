using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.InMemory.DbMemory;
using Microsoft.Extensions.DependencyInjection;

namespace FluentHelper.EntityFrameworkCore.InMemory
{
    public static class InMemoryProviderExtensions
    {
        public static IServiceCollection AddInMemoryContext(this IServiceCollection serviceCollection)
        {
            DbMemoryContext dbMemoryContext = DbMemoryContext.GetOrCreate();
            serviceCollection.AddFluentDbContext(dbMemoryContext.DbContext);

            return serviceCollection;
        }

        public static void AddMemoryContextSupportTo<T>(IEnumerable<T>? initialData = null) where T : class
        {
            DbMemoryContext dbMemoryContext = DbMemoryContext.GetOrCreate();
            dbMemoryContext.AddSupportTo(initialData);
        }

        public static void ClearMemoryContext()
        {
            DbMemoryContext dbMemoryContext = DbMemoryContext.GetOrCreate();
            dbMemoryContext.Dispose();
        }
    }
}
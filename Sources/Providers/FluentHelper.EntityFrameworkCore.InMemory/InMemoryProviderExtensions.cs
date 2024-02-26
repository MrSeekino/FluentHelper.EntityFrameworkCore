using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.InMemory.DbMemory;
using Microsoft.Extensions.DependencyInjection;

namespace FluentHelper.EntityFrameworkCore.InMemory
{
    public static class InMemoryProviderExtensions
    {
        /// <summary>
        /// Add a custom InMemory context using NSubstitute
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection AddInMemoryContext(this IServiceCollection serviceCollection)
        {
            DbMemoryContext dbMemoryContext = DbMemoryContext.GetOrCreate();
            serviceCollection.AddFluentDbContext(dbMemoryContext.DbContext);

            return serviceCollection;
        }

        /// <summary>
        /// Add support to a specified model on the InMemory context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initialData">the initial data the model should have</param>
        public static void AddMemoryContextSupportTo<T>(IEnumerable<T>? initialData = null) where T : class
        {
            DbMemoryContext dbMemoryContext = DbMemoryContext.GetOrCreate();
            dbMemoryContext.AddSupportTo(initialData);
        }

        /// <summary>
        /// Dispose the current InMemory context
        /// </summary>
        public static void ClearMemoryContext()
        {
            DbMemoryContext dbMemoryContext = DbMemoryContext.GetOrCreate();
            dbMemoryContext.Dispose();
        }
    }
}
using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace FluentHelper.EntityFrameworkCore.Common
{
    public static class EfDbDependencyInjectionExtensions
    {
        /// <summary>
        /// Add FluentDbContext to the dependency injection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="dbConfigBuilderFunc">The action to set all the configurations</param>
        /// <param name="serviceLifetime">The Lifetime of the context. Default to Transient</param>
        public static void AddFluentDbContext(this IServiceCollection serviceCollection, Action<EfDbConfigBuilder> dbConfigBuilderFunc, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            EfDbConfigBuilder dbConfigBuilder = new EfDbConfigBuilder();
            dbConfigBuilderFunc(dbConfigBuilder);

            IDbConfig dbConfig = dbConfigBuilder.Build();
            serviceCollection.AddSingleton(x => dbConfig);

            var mappingTypes = dbConfig.MappingAssemblies.SelectMany(m => m.GetTypes()).Where(p => p.IsClass && typeof(IDbMap).IsAssignableFrom(p) && !p.IsAbstract).ToList();
            foreach (var mappingType in mappingTypes)
                serviceCollection.AddSingleton(typeof(IDbMap), mappingType!);

            serviceCollection.Add(new ServiceDescriptor(typeof(IDbContext), typeof(EfDbContext), serviceLifetime));
        }

        /// <summary>
        /// Add the specified dbContext as a singleton
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="dbContext"></param>
        public static void AddFluentDbContext(this IServiceCollection serviceCollection, IDbContext dbContext)
        {
            serviceCollection.AddSingleton(typeof(IDbContext), dbContext);
        }
    }
}

using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace FluentHelper.EntityFrameworkCore.Common
{
    public static class EfDbDependencyInjectionExtensions
    {
        public static void AddFluentDbContext(this IServiceCollection serviceCollection, Action<EfDbConfigBuilder> dbConfigBuilderFunc, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
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
    }
}

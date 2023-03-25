using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace FluentHelper.EntityFrameworkCore.Common
{
    public static class DependencyInjectionExtensions
    {
        public static void AddFluentDbContext(this IServiceCollection serviceCollection, Action<EfDbConfigBuilder> dbConfigBuilderFunc)
        {
            EfDbConfigBuilder dbConfigBuilder = new EfDbConfigBuilder();
            dbConfigBuilderFunc(dbConfigBuilder);

            IDbConfig dbConfig = dbConfigBuilder.Build();
            serviceCollection.AddSingleton(x => dbConfig);

            var mappingTypes = dbConfig.MappingAssemblies.SelectMany(m => m.GetTypes()).Where(p => p.IsClass && typeof(IDbMap).IsAssignableFrom(p) && !p.IsAbstract).ToList();
            foreach (var mappingType in mappingTypes)
                serviceCollection.AddSingleton(typeof(IDbMap), mappingType!);

            serviceCollection.AddSingleton<IDbContext, EfDbContext>();
        }
    }
}

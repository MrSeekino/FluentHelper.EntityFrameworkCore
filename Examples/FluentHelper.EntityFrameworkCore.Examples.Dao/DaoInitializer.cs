using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Examples.Mappings;
using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.Extensions.Configuration;
using System;

namespace FluentHelper.EntityFrameworkCore.Examples.Dao
{
    public static class DaoInitializer
    {
        static IConfiguration Configuration { get; }

        static DaoInitializer()
        {
            Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
        }

        public static IDbContext InitializeContext()
        {
            return EfDbContextManager.GenerateContext()
                .WithSqlDbProvider(Configuration.GetConnectionString("FluentHelperExampleConnectionString"))
                //.WithDbProviderConfiguration(new SqlDbProviderConfiguration(Configuration.GetConnectionString("FluentHelperExampleConnectionString")))
                //.WithConnectionString(Configuration.GetConnectionString("FluentHelperExampleConnectionString"))
                .WithLogAction((logLevel, eventId, message) => Console.WriteLine($"{logLevel} | {eventId}: {message}"), true)
                .WithLazyLoadingProxies()
                .WithMappingFromAssemblyOf<TestDataMap>();
        }
    }
}

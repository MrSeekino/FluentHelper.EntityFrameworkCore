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
                .SetConnectionString(Configuration.GetConnectionString("FluentHelperExampleConnectionString"))
                .SetLogAction(x => Console.WriteLine(x), true)
                .UseLazyLoadingProxies()
                .AddMappingFromAssemblyOf<TestDataMap>();
        }
    }
}

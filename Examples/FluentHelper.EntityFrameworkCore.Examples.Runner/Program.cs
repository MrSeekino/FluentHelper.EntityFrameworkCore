using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Examples.Mappings;
using FluentHelper.EntityFrameworkCore.Examples.Repositories;
using FluentHelper.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FluentHelper.EntityFrameworkCore.Examples.Runner
{
    class Program
    {
        static IConfiguration Configuration { get; }
        static ServiceProvider? ServiceProvider { get; set; }

        static Program()
        {
            Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
            ServiceProvider = null;
        }

        static void Main()
        {
            Program program = new();
            program.StartProgram();
        }

        private void StartProgram()
        {
            SetupDependencyInjection();

            ExampleRunner? exampleRunner = ServiceProvider?.GetRequiredService<ExampleRunner>();
            exampleRunner?.Start();
        }

        private void SetupDependencyInjection()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddFluentDbContext(dbConfigBuilder =>
            {
                dbConfigBuilder.WithSqlDbProvider(Configuration.GetConnectionString("FluentHelperExampleConnectionString"))
                        .WithLogAction((logLevel, eventId, message) => Console.WriteLine($"{logLevel} | {eventId}: {message}"), true)
                        .WithLazyLoadingProxies()
                        .WithMappingFromAssemblyOf<TestDataMap>();
            });

            serviceCollection.AddSingleton<TestDataRepository, TestDataRepository>();
            serviceCollection.AddSingleton<ExampleRunner, ExampleRunner>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}

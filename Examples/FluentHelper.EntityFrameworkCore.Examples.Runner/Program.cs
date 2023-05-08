using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Examples.Mappings;
using FluentHelper.EntityFrameworkCore.Examples.Repositories;
using FluentHelper.EntityFrameworkCore.SqlServer;
using FluentHelper.EntityFramworkCore.PostgreSql;
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
            Console.WriteLine("Choose the provider:");
            Console.WriteLine("1. SqlServer");
            Console.WriteLine("2. Postgres");

            string? chosenProvider = Console.ReadLine();
            switch (chosenProvider)
            {
                case "1":
                case "SqlServer":
                    SetupDependencyInjection(ProviderChoice.SqlServer);
                    break;
                case "2":
                case "Postgres":
                    SetupDependencyInjection(ProviderChoice.PostgreSql);
                    break;
                default:
                    Console.WriteLine($"Provider {chosenProvider} not recognized");
                    break;
            }

            ExampleRunner? exampleRunner = ServiceProvider?.GetRequiredService<ExampleRunner>();
            exampleRunner?.Start();
        }

        private void SetupDependencyInjection(ProviderChoice providerChoice)
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddFluentDbContext(dbConfigBuilder =>
            {
                if (providerChoice == ProviderChoice.SqlServer)
                    dbConfigBuilder.WithSqlDbProvider(Configuration.GetConnectionString("FluentHelperExampleConnectionStringSqlServer")!)
                            .WithLogAction((logLevel, eventId, message) => Console.WriteLine($"{logLevel} | {eventId}: {message}"), true)
                            .WithLazyLoadingProxies()
                            .WithMappingFromAssemblyOf<TestDataMap>();

                if (providerChoice == ProviderChoice.PostgreSql)
                    dbConfigBuilder.WithPostgreSqlProvider(Configuration.GetConnectionString("FluentHelperExampleConnectionStringPostgreSql")!)
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

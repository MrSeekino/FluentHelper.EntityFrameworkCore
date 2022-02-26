using EntityFramework.FluentHelperCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EntityFramework.FluentHelperCore.Common
{
    class EfDbModel : DbContext
    {
        string ConnectionString { get; set; }

        Action<string> LogAction { get; set; }
        Func<EventId, LogLevel, bool> LogFilter { get; set; }
        bool EnableSensitiveDataLogging { get; set; }

        bool EnableLazyLoadingProxies { get; set; }

        List<Assembly> MappingAssemblies { get; set; }

        public EfDbModel(string connectionString, Action<string> logAction, Func<EventId, LogLevel, bool> logFilter, bool enableSensitiveDataLogging, bool enableLazyLoadingProxies) : base()
        {
            ConnectionString = connectionString;

            LogAction = logAction;
            LogFilter = logFilter;
            EnableSensitiveDataLogging = enableSensitiveDataLogging;
            EnableLazyLoadingProxies = enableLazyLoadingProxies;

            MappingAssemblies = new List<Assembly>();
        }

        public void AddMappingAssembly(Assembly mappingAssembly)
        {
            MappingAssemblies.Add(mappingAssembly);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(ConnectionString);
                optionsBuilder.LogTo(LogAction, LogFilter);

                if (EnableSensitiveDataLogging)
                    optionsBuilder.EnableSensitiveDataLogging();

                if (EnableLazyLoadingProxies)
                    optionsBuilder.UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var mappings = MappingAssemblies.SelectMany(m => m.GetTypes()).Where(p => p.IsClass && typeof(IDbMap).IsAssignableFrom(p) && !p.IsAbstract).ToList();

            foreach (var m in mappings)
                ((IDbMap)((EfDbMap)Activator.CreateInstance(m)).SetModelBuilder(modelBuilder)).Map();
        }
    }
}

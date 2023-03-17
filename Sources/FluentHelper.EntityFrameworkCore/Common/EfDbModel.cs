using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FluentHelper.EntityFrameworkCore.Tests")]
namespace FluentHelper.EntityFrameworkCore.Common
{
    class EfDbModel : DbContext
    {
        internal IDbProviderConfiguration? DbProviderConfiguration { get; set; }
        internal Action<LogLevel, EventId, string>? LogAction { get; set; }

        internal bool EnableSensitiveDataLogging { get; set; }
        internal bool EnableLazyLoadingProxies { get; set; }

        internal Func<DbContextOptionsBuilder, DbContextOptionsBuilder>? UseLazyLoadingProxiesBehaviour { get; set; }
        internal Func<Type, IDbMap>? GetInstanceOfDbMapBehaviour { get; set; }

        internal List<Assembly> MappingAssemblies { get; set; } = new List<Assembly>();

        public EfDbModel(IDbProviderConfiguration dbProviderConfiguration, Action<LogLevel, EventId, string>? logAction, bool enableSensitiveDataLogging, bool enableLazyLoadingProxies)
            : this(dbProviderConfiguration, logAction, enableSensitiveDataLogging, enableLazyLoadingProxies,
                  (ob) => ob.UseLazyLoadingProxies(),
                  (x) => { return (IDbMap)Activator.CreateInstance(x)!; })
        { }

        internal EfDbModel(IDbProviderConfiguration dbProviderConfiguration, Action<LogLevel, EventId, string>? logAction, bool enableSensitiveDataLogging, bool enableLazyLoadingProxies,
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> useLazyLoadingProxiesBehaviour,
            Func<Type, IDbMap> getInstanceOfDbMapBehaviour) : base()
        {
            DbProviderConfiguration = dbProviderConfiguration;

            LogAction = logAction;
            EnableSensitiveDataLogging = enableSensitiveDataLogging;
            EnableLazyLoadingProxies = enableLazyLoadingProxies;

            MappingAssemblies = new List<Assembly>();

            UseLazyLoadingProxiesBehaviour = useLazyLoadingProxiesBehaviour;
            GetInstanceOfDbMapBehaviour = getInstanceOfDbMapBehaviour;
        }

        internal EfDbModel() { }

        public void AddMappingAssembly(Assembly mappingAssembly)
        {
            if (MappingAssemblies == null)
                MappingAssemblies = new List<Assembly>();

            MappingAssemblies.Add(mappingAssembly);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Configure(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CreateModel(modelBuilder);
        }

        internal void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                DbProviderConfiguration!.ConfigureDbProvider(optionsBuilder);

                if (LogAction != null)
                    optionsBuilder.LogTo((e, l) => true, eventData => LogAction(eventData.LogLevel, eventData.EventId, eventData.ToString()));

                if (EnableSensitiveDataLogging)
                    optionsBuilder.EnableSensitiveDataLogging();

                if (EnableLazyLoadingProxies)
                    UseLazyLoadingProxiesBehaviour!(optionsBuilder);
            }
        }

        internal void CreateModel(ModelBuilder modelBuilder)
        {
            var mappings = MappingAssemblies.SelectMany(m => m.GetTypes())
                            .Where(p => p.IsClass && typeof(IDbMap).IsAssignableFrom(p) && !p.IsAbstract).ToList();

            foreach (var m in mappings)
            {
                var objInstance = GetInstanceOfDbMapBehaviour!(m);
                objInstance.SetModelBuilder(modelBuilder);
                objInstance.Map();
            }
        }
    }
}

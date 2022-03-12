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
        internal string ConnectionString { get; set; }

        internal Action<string> LogAction { get; set; }
        internal Func<EventId, LogLevel, bool> LogFilter { get; set; }

        internal bool EnableSensitiveDataLogging { get; set; }
        internal bool EnableLazyLoadingProxies { get; set; }

        internal Func<DbContextOptionsBuilder, string, DbContextOptionsBuilder> UseSqlServerBehaviour { get; set; }
        internal Func<DbContextOptionsBuilder, DbContextOptionsBuilder> UseLazyLoadingProxiesBehaviour { get; set; }
        internal Func<Type, IDbMap> GetInstanceOfDbMapBehaviour { get; set; }

        internal List<Assembly> MappingAssemblies { get; set; }

        public EfDbModel(string connectionString, Action<string> logAction, Func<EventId, LogLevel, bool> logFilter, bool enableSensitiveDataLogging, bool enableLazyLoadingProxies)
            : this(connectionString, logAction, logFilter, enableSensitiveDataLogging, enableLazyLoadingProxies,
                  (ob, cs) => ob.UseSqlServer(cs),
                  (ob) => ob.UseLazyLoadingProxies(),
                  (x) => { return (IDbMap)Activator.CreateInstance(x); })
        { }

        internal EfDbModel(string connectionString, Action<string> logAction, Func<EventId, LogLevel, bool> logFilter, bool enableSensitiveDataLogging, bool enableLazyLoadingProxies,
            Func<DbContextOptionsBuilder, string, DbContextOptionsBuilder> useSqlServerBehaviour,
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> useLazyLoadingProxiesBehaviour,
            Func<Type, IDbMap> getInstanceOfDbMapBehaviour) : base()
        {
            ConnectionString = connectionString;

            LogAction = logAction;
            LogFilter = logFilter;
            EnableSensitiveDataLogging = enableSensitiveDataLogging;
            EnableLazyLoadingProxies = enableLazyLoadingProxies;

            MappingAssemblies = new List<Assembly>();

            UseSqlServerBehaviour = useSqlServerBehaviour;
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
                UseSqlServerBehaviour(optionsBuilder, ConnectionString);

                optionsBuilder.LogTo(LogAction, LogFilter);

                if (EnableSensitiveDataLogging)
                    optionsBuilder.EnableSensitiveDataLogging();

                if (EnableLazyLoadingProxies)
                    UseLazyLoadingProxiesBehaviour(optionsBuilder);
            }
        }

        internal void CreateModel(ModelBuilder modelBuilder)
        {
            var mappings = MappingAssemblies.SelectMany(m => m.GetTypes())
                            .Where(p => p.IsClass && typeof(IDbMap).IsAssignableFrom(p) && !p.IsAbstract).ToList();

            foreach (var m in mappings)
            {
                var objInstance = GetInstanceOfDbMapBehaviour(m);
                objInstance.SetModelBuilder(modelBuilder);
                objInstance.Map();
            }
        }
    }
}

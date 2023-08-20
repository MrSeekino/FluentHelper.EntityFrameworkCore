using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FluentHelper.EntityFrameworkCore.Tests")]
namespace FluentHelper.EntityFrameworkCore.Common
{
    internal class EfDbModel : DbContext
    {
        internal IDbConfig DbConfig { get; set; }
        internal IEnumerable<IDbMap> Mappings { get; set; }

        internal Action<DbContextOptionsBuilder> UseLazyLoadingProxiesBehaviour { get; set; }

        public EfDbModel(IDbConfig dbConfig, IEnumerable<IDbMap> mappings)
            : this(dbConfig, mappings, optionsBuilder => { optionsBuilder.UseLazyLoadingProxies(); })
        { }

        public EfDbModel(IDbConfig dbConfig, IEnumerable<IDbMap> mappings, Action<DbContextOptionsBuilder> useLazyLoadingProxiesBehaviour)
        {
            DbConfig = dbConfig;
            Mappings = mappings;

            UseLazyLoadingProxiesBehaviour = useLazyLoadingProxiesBehaviour;
        }

        [ExcludeFromCodeCoverage]
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Configure(optionsBuilder);
        }

        [ExcludeFromCodeCoverage]
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CreateModel(modelBuilder);
        }

        internal void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (DbConfig.DbProviderConfiguration == null)
                    throw new NullReferenceException($"Unspecified DbProvider");

                DbConfig.DbProviderConfiguration!(optionsBuilder);

                if (DbConfig.LogAction != null)
                    optionsBuilder.LogTo((e, l) => true, eventData => DbConfig.LogAction(eventData.LogLevel, eventData.EventId, eventData.ToString()));

                if (DbConfig.EnableSensitiveDataLogging)
                    optionsBuilder.EnableSensitiveDataLogging();

                if (DbConfig.EnableLazyLoadingProxies)
                    UseLazyLoadingProxiesBehaviour(optionsBuilder);
            }
        }

        internal void CreateModel(ModelBuilder modelBuilder)
        {
            foreach (var mapInstance in Mappings)
            {
                mapInstance.SetModelBuilder(modelBuilder);
                mapInstance.Map();
            }
        }
    }
}

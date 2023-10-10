using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FluentHelper.EntityFrameworkCore.Tests")]
namespace FluentHelper.EntityFrameworkCore.Common
{
    internal class EfDbModel : DbContext
    {
        private IDbConfig _dbConfig;
        private IEnumerable<IDbMap> _mappings;
        private Action<DbContextOptionsBuilder> _useLazyLoadingProxiesBehaviour;

        internal int MappingsLength => _mappings.Count();

        public EfDbModel(IDbConfig dbConfig, IEnumerable<IDbMap> mappings)
            : this(dbConfig, mappings, optionsBuilder => { optionsBuilder.UseLazyLoadingProxies(); })
        { }

        public EfDbModel(IDbConfig dbConfig, IEnumerable<IDbMap> mappings, Action<DbContextOptionsBuilder> useLazyLoadingProxiesBehaviour)
        {
            _dbConfig = dbConfig;
            _mappings = mappings;
            _useLazyLoadingProxiesBehaviour = useLazyLoadingProxiesBehaviour;
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
                if (_dbConfig.DbProvider == null)
                    throw new ArgumentNullException("Unspecified DbProvider");

                if (_dbConfig.DbConfiguration != null)
                    _dbConfig.DbConfiguration!(optionsBuilder);

                _dbConfig.DbProvider!(optionsBuilder);

                if (_dbConfig.LogAction != null)
                    optionsBuilder.LogTo((e, l) => true, eventData => _dbConfig.LogAction(eventData.LogLevel, eventData.EventId, eventData.ToString()));

                if (_dbConfig.EnableSensitiveDataLogging)
                    optionsBuilder.EnableSensitiveDataLogging();

                if (_dbConfig.EnableLazyLoadingProxies)
                    _useLazyLoadingProxiesBehaviour(optionsBuilder);
            }
        }

        internal void CreateModel(ModelBuilder modelBuilder)
        {
            foreach (var mapInstance in _mappings)
            {
                mapInstance.SetModelBuilder(modelBuilder);
                mapInstance.Map();
            }
        }
    }
}

﻿using FluentHelper.EntityFrameworkCore.Interfaces;
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
        private readonly IDbConfig _dbConfig;
        private readonly IEnumerable<IDbMap> _mappings;

        internal int MappingsLength => _mappings.Count();

        public EfDbModel(IDbConfig dbConfig, IEnumerable<IDbMap> mappings)
        {
            _dbConfig = dbConfig;
            _mappings = mappings;
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
                    throw new InvalidOperationException("Unspecified DbProvider");

                if (_dbConfig.DbConfiguration != null)
                    _dbConfig.DbConfiguration!(optionsBuilder);

                _dbConfig.DbProvider!(optionsBuilder);

                if (_dbConfig.LogAction != null)
                    optionsBuilder.LogTo((e, l) => true, eventData => _dbConfig.LogAction(eventData.LogLevel, eventData.EventId, eventData.ToString()));

                if (_dbConfig.EnableSensitiveDataLogging)
                    optionsBuilder.EnableSensitiveDataLogging();

                if (_dbConfig.LazyLoadingProxiesBehaviour != null)
                    _dbConfig.LazyLoadingProxiesBehaviour(optionsBuilder);
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

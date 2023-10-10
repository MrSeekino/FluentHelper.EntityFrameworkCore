using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentHelper.EntityFrameworkCore.Common
{
    public sealed class EfDbConfigBuilder
    {
        private Action<DbContextOptionsBuilder>? _dbConfiguration;
        private Action<DbContextOptionsBuilder>? _dbProvider;
        private Action<LogLevel, EventId, string>? _logAction;
        private bool _enableSensitiveDataLogging;
        private bool enableLazyLoadingProxies;
        private List<Assembly> _mappingAssemblies;

        public EfDbConfigBuilder()
        {
            _dbConfiguration = null;
            _dbProvider = null;
            _logAction = null;
            _enableSensitiveDataLogging = false;
            _enableSensitiveDataLogging = false;
            _mappingAssemblies = new List<Assembly>();
        }

        public EfDbConfigBuilder WithDbConfiguration(Action<DbContextOptionsBuilder> dbConfiguration)
        {
            _dbConfiguration = dbConfiguration;
            return this;
        }

        public EfDbConfigBuilder WithDbProvider(Action<DbContextOptionsBuilder> dbProvider)
        {
            this._dbProvider = dbProvider;
            return this;
        }

        public EfDbConfigBuilder WithLazyLoadingProxies()
        {
            enableLazyLoadingProxies = true;
            return this;
        }

        public EfDbConfigBuilder WithMappingFromAssemblyOf<T>()
        {
            var mappingAssembly = Assembly.GetAssembly(typeof(T)) ?? throw new ArgumentException($"Could not find assembly with {typeof(T).Name}");

            _mappingAssemblies.Add(mappingAssembly!);
            return this;
        }

        public EfDbConfigBuilder WithLogAction(Action<LogLevel, EventId, string> logAction, bool enableSensitiveDataLogging = false)
        {
            _logAction = logAction;
            _enableSensitiveDataLogging = enableSensitiveDataLogging;
            return this;
        }

        public IDbConfig Build()
        {
            return new DbConfig
            {
                DbConfiguration = _dbConfiguration,
                DbProvider = _dbProvider,
                LogAction = _logAction,
                EnableSensitiveDataLogging = _enableSensitiveDataLogging,
                EnableLazyLoadingProxies = enableLazyLoadingProxies,
                MappingAssemblies = _mappingAssemblies
            };
        }
    }
}

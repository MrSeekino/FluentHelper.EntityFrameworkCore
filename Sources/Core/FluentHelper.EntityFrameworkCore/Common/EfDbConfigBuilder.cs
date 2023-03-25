using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentHelper.EntityFrameworkCore.Common
{
    public class EfDbConfigBuilder
    {
        internal Action<DbContextOptionsBuilder>? DbProviderConfiguration { get; set; }
        internal Action<LogLevel, EventId, string>? LogAction { get; set; }

        internal bool EnableSensitiveDataLogging { get; set; }
        internal bool EnableLazyLoadingProxies { get; set; }

        internal List<Assembly> MappingAssemblies { get; set; } = new List<Assembly>();

        public EfDbConfigBuilder WithDbProviderConfiguration(Action<DbContextOptionsBuilder> dbProviderConfiguration)
        {
            DbProviderConfiguration = dbProviderConfiguration;
            return this;
        }

        public EfDbConfigBuilder WithLazyLoadingProxies()
        {
            EnableLazyLoadingProxies = true;
            return this;
        }

        public EfDbConfigBuilder WithMappingFromAssemblyOf<T>()
        {
            var mappingAssembly = Assembly.GetAssembly(typeof(T)) ?? throw new ArgumentException($"Could not find assembly with {typeof(T).Name}");

            MappingAssemblies.Add(mappingAssembly!);
            return this;
        }

        public EfDbConfigBuilder WithLogAction(Action<LogLevel, EventId, string> logAction, bool enableSensitiveDataLogging = false)
        {
            LogAction = logAction;
            EnableSensitiveDataLogging = enableSensitiveDataLogging;
            return this;
        }

        public IDbConfig Build()
        {
            return new DbConfig
            {
                DbProviderConfiguration = DbProviderConfiguration,
                LogAction = LogAction,
                EnableSensitiveDataLogging = EnableLazyLoadingProxies,
                EnableLazyLoadingProxies = EnableLazyLoadingProxies,
                MappingAssemblies = MappingAssemblies
            };
        }
    }
}

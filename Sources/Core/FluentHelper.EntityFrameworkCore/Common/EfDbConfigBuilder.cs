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
        private Action<DbContextOptionsBuilder>? _lazyLoadingProxiesBehaviour;
        private bool _enableSensitiveDataLogging;
        private readonly List<Assembly> _mappingAssemblies;

        public EfDbConfigBuilder()
        {
            _dbConfiguration = null;
            _dbProvider = null;
            _logAction = null;
            _enableSensitiveDataLogging = false;
            _enableSensitiveDataLogging = false;
            _mappingAssemblies = new List<Assembly>();
        }

        /// <summary>
        /// Set specific custom configuration for the database
        /// </summary>
        /// <param name="dbConfiguration"></param>
        /// <returns></returns>
        public EfDbConfigBuilder WithDbConfiguration(Action<DbContextOptionsBuilder> dbConfiguration)
        {
            _dbConfiguration = dbConfiguration;
            return this;
        }

        /// <summary>
        /// Set the DbProvider to be used
        /// </summary>
        /// <param name="dbProvider"></param>
        /// <returns></returns>
        public EfDbConfigBuilder WithDbProvider(Action<DbContextOptionsBuilder> dbProvider)
        {
            _dbProvider = dbProvider;
            return this;
        }

        /// <summary>
        /// Enable LazyLoadingProxies
        /// </summary>
        /// <param name="lazyLoadingProxiesOptionsAction"></param>
        /// <returns></returns>
        public EfDbConfigBuilder WithLazyLoadingProxies(Action<LazyLoadingProxiesOptionsBuilder>? lazyLoadingProxiesOptionsAction = null)
        {
            _lazyLoadingProxiesBehaviour = lazyLoadingProxiesOptionsAction != null ? optionsBuilder => optionsBuilder.UseLazyLoadingProxies(lazyLoadingProxiesOptionsAction!) : optionsBuilder => optionsBuilder.UseLazyLoadingProxies();
            return this;
        }

        /// <summary>
        /// Add all the mappings extending 'EfDbMap<T>' found on the assembly where the specified 'T' type is found. Throws if the 'T' type is not found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public EfDbConfigBuilder WithMappingFromAssemblyOf<T>()
        {
            var mappingAssembly = Assembly.GetAssembly(typeof(T)) ?? throw new ArgumentException($"Could not find assembly with {typeof(T).Name}");

            _mappingAssemblies.Add(mappingAssembly!);
            return this;
        }

        /// <summary>
        /// Set a function to be called for internal logging
        /// </summary>
        /// <param name="logAction"></param>
        /// <param name="enableSensitiveDataLogging"></param>
        /// <returns></returns>
        public EfDbConfigBuilder WithLogAction(Action<LogLevel, EventId, string> logAction, bool enableSensitiveDataLogging = false)
        {
            _logAction = logAction;
            _enableSensitiveDataLogging = enableSensitiveDataLogging;
            return this;
        }

        /// <summary>
        /// Build the IDbConfig object with current set configurations
        /// </summary>
        /// <returns></returns>
        public IDbConfig Build()
        {
            return new DbConfig
            {
                DbConfiguration = _dbConfiguration,
                DbProvider = _dbProvider,
                LogAction = _logAction,
                EnableSensitiveDataLogging = _enableSensitiveDataLogging,
                LazyLoadingProxiesBehaviour = _lazyLoadingProxiesBehaviour,
                MappingAssemblies = _mappingAssemblies
            };
        }
    }
}

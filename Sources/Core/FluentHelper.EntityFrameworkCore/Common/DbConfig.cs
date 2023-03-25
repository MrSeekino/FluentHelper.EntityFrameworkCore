using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FluentHelper.EntityFrameworkCore.Common
{
    [ExcludeFromCodeCoverage]
    internal class DbConfig : IDbConfig
    {
        public Action<DbContextOptionsBuilder>? DbProviderConfiguration { get; set; }
        public Action<LogLevel, EventId, string>? LogAction { get; set; }

        public bool EnableSensitiveDataLogging { get; set; }
        public bool EnableLazyLoadingProxies { get; set; }

        public List<Assembly> MappingAssemblies { get; set; } = new List<Assembly>();
    }
}

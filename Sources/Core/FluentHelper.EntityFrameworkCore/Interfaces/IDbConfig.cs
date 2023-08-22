using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentHelper.EntityFrameworkCore.Interfaces
{
    public interface IDbConfig
    {
        internal Action<DbContextOptionsBuilder>? DbConfiguration { get; }
        Action<DbContextOptionsBuilder>? DbProvider { get; }
        Action<LogLevel, EventId, string>? LogAction { get; }

        bool EnableSensitiveDataLogging { get; }
        bool EnableLazyLoadingProxies { get; }

        List<Assembly> MappingAssemblies { get; }
    }
}

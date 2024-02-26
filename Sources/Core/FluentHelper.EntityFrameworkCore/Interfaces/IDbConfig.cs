using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentHelper.EntityFrameworkCore.Interfaces
{
    public interface IDbConfig
    {
        Action<DbContextOptionsBuilder>? DbConfiguration { get; }
        Action<DbContextOptionsBuilder>? DbProvider { get; }
        Action<LogLevel, EventId, string>? LogAction { get; }

        Action<DbContextOptionsBuilder>? LazyLoadingProxiesBehaviour { get; }
        bool EnableSensitiveDataLogging { get; }

        List<Assembly> MappingAssemblies { get; }
    }
}

﻿using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentHelper.EntityFrameworkCore.Common
{
    internal sealed class DbConfig : IDbConfig
    {
        public Action<DbContextOptionsBuilder>? DbConfiguration { get; set; }
        public Action<DbContextOptionsBuilder>? DbProvider { get; set; }
        public Action<LogLevel, EventId, string>? LogAction { get; set; }

        public Action<DbContextOptionsBuilder>? LazyLoadingProxiesBehaviour { get; set; }
        public bool EnableSensitiveDataLogging { get; set; } = false;

        public List<Assembly> MappingAssemblies { get; set; } = new List<Assembly>();
    }
}

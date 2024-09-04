using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FluentHelper.EntityFrameworkCore.Tests.Support
{
    internal class TestEfDbModel : EfDbModel
    {
        public TestEfDbModel(ILoggerFactory loggerFactory, IDbConfig dbConfig, IEnumerable<IDbMap> mappings)
            : base(loggerFactory, dbConfig, mappings) { }

        public void OnConfiguringWrapper(DbContextOptionsBuilder optionsBuilder)
        {
            OnConfiguring(optionsBuilder);
        }

        public void OnModelCreatingWrapper(ModelBuilder modelBuilder)
        {
            OnModelCreating(modelBuilder);
        }
    }
}

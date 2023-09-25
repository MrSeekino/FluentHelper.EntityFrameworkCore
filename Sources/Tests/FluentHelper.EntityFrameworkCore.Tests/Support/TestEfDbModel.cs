using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FluentHelper.EntityFrameworkCore.Tests.Support
{
    internal class TestEfDbModel : EfDbModel
    {
        public TestEfDbModel(IDbConfig dbConfig, IEnumerable<IDbMap> mappings) 
            : base(dbConfig, mappings)  { }

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

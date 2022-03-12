using FluentHelper.EntityFrameworkCore.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHelper.EntityFrameworkCore.Tests.Support
{
    public class TestEntityMap : EfDbMap<TestEntity>
    {
        public override void Map()
        {
            Entity.ToTable("TestDataTable");

            Entity.HasKey(e => e.Id);

            Entity.Property(e => e.Name);
        }
    }
}

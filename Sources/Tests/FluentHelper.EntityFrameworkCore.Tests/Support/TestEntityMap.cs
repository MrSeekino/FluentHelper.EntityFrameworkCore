using FluentHelper.EntityFrameworkCore.Common;
using Microsoft.EntityFrameworkCore;

namespace FluentHelper.EntityFrameworkCore.Tests.Support
{
    public class TestEntityMap : EfDbMap<TestEntity>
    {
        public override void Map()
        {
            Entity.ToTable("Test");

            Entity.HasKey(t => t.Id);

            Entity.Property(t => t.Name);
            Entity.Property(t => t.Description);
        }
    }
}

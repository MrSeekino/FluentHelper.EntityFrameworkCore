using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Examples.Models;
using Microsoft.EntityFrameworkCore;

namespace FluentHelper.EntityFrameworkCore.Examples.Mappings
{
    public class TestDataMap : EfDbMap<TestData>
    {
        public override void Map()
        {
            Entity.ToTable("TestDataTable");

            Entity.HasKey(e => e.Id);

            Entity.Property(e => e.Name);
            Entity.Property(e => e.CreationDate);
            Entity.Property(e => e.Active);

            Entity.HasOne(e => e.Attr).WithOne(e => e.Data);
            Entity.HasMany(e => e.ChildList).WithOne(e => e.Parent).HasForeignKey(e => e.IdParent);
        }
    }
}

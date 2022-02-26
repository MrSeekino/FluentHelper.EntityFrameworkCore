using EntityFramework.FluentHelperCore.Common;
using EntityFramework.FluentHelperCore.Examples.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.FluentHelperCore.Examples.Mappings
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

            Entity.HasMany(e => e.ChildList).WithOne(e => e.Parent).HasForeignKey(e => e.IdParent);
        }
    }
}

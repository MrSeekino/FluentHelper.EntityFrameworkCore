using EntityFramework.FluentHelperCore.Common;
using EntityFramework.FluentHelperCore.Examples.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.FluentHelperCore.Examples.Mappings
{
    public class TestChildMap : EfDbMap<TestChild>
    {
        public override void Map()
        {
            Entity.ToTable("TestDataChild");

            Entity.HasKey(e => e.Id);

            Entity.Property(e => e.IdParent);
            Entity.Property(e => e.Name);
            Entity.Property(e => e.CreationDate);
            Entity.Property(e => e.Active);

            Entity.HasOne(e => e.Parent).WithMany(e => e.ChildList).HasForeignKey(e => e.IdParent);
        }
    }
}

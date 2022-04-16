using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Examples.Models;
using Microsoft.EntityFrameworkCore;

namespace FluentHelper.EntityFrameworkCore.Examples.Mappings
{
    public class TestDataAttrMap : EfDbMap<TestDataAttr>
    {
        public override void Map()
        {
            Entity.ToTable("TestDataAttr");

            Entity.HasKey(e => e.Id);

            Entity.Property(e => e.IsBeautiful);

            Entity.HasOne(e => e.Data).WithOne(e => e.Attr).HasForeignKey<TestDataAttr>(e => e.Id);
        }
    }
}

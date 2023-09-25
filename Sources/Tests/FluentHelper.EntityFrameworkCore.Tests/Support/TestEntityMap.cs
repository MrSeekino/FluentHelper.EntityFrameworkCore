using FluentHelper.EntityFrameworkCore.Common;
using Microsoft.EntityFrameworkCore;

namespace FluentHelper.EntityFrameworkCore.Tests.Support
{
    public class TestEntityMap : EfDbMap<TestEntity>
    {
        ModelBuilder? ModelBuilder { get; set; } = new ModelBuilder();

        public override void Map()
        {
        }
    }
}

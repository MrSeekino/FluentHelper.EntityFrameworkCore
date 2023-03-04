using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace FluentHelper.EntityFrameworkCore.Common
{
    [ExcludeFromCodeCoverage]
    public abstract class EfDbMap : IDbMap
    {
        ModelBuilder ModelBuilder { get; set; }

        public ModelBuilder GetModelBuilder()
        {
            return ModelBuilder;
        }

        public void SetModelBuilder(ModelBuilder modelBuilder)
        {
            ModelBuilder = modelBuilder;
        }

        public abstract void Map();
    }

    [ExcludeFromCodeCoverage]
    public abstract class EfDbMap<T> : EfDbMap, IDbMap where T : class
    {
        public EntityTypeBuilder<T> Entity
        {
            get
            {
                return GetModelBuilder().Entity<T>();
            }
        }
    }
}

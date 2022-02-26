using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluentHelper.EntityFrameworkCore.Common
{
    public abstract class EfDbMap
    {
        ModelBuilder ModelBuilder { get; set; }

        public ModelBuilder GetModelBuilder()
        {
            return ModelBuilder;
        }

        public EfDbMap SetModelBuilder(ModelBuilder modelBuilder)
        {
            ModelBuilder = modelBuilder;

            return this;
        }
    }

    public abstract class EfDbMap<T> : EfDbMap, IDbMap where T : class
    {
        public EntityTypeBuilder<T> Entity
        {
            get
            {
                return GetModelBuilder().Entity<T>();
            }
        }

        public abstract void Map();
    }
}

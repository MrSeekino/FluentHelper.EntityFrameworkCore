using EntityFramework.FluentHelperCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFramework.FluentHelperCore.Common
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

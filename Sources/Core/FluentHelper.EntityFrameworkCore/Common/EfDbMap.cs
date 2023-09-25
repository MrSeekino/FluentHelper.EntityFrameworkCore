using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace FluentHelper.EntityFrameworkCore.Common
{
    public abstract class EfDbMap<T> : IDbMap where T : class
    {
        ModelBuilder? ModelBuilder { get; set; }

        public EntityTypeBuilder<T> Entity => GetModelBuilder().Entity<T>();

        public ModelBuilder GetModelBuilder()
        {
            if (ModelBuilder == null)
                throw new ArgumentNullException("ModelBuilder has not been set");

            return ModelBuilder;
        }

        public void SetModelBuilder(ModelBuilder modelBuilder)
        {
            ModelBuilder = modelBuilder;
        }

        public Type GetMappedType()
        {
            return typeof(T);
        }

        public abstract void Map();
    }
}

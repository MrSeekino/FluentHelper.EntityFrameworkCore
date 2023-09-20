using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace FluentHelper.EntityFrameworkCore.Common
{
    public abstract class EfDbMap<T> : IDbMap where T : class
    {
        ModelBuilder? ModelBuilder { get; set; }

        public EntityTypeBuilder<T> Entity
        {
            get
            {
                return GetModelBuilder().Entity<T>();
            }
        }

        public ModelBuilder GetModelBuilder()
        {
            if (ModelBuilder == null)
                throw new NullReferenceException("ModelBuilder has not been set");

            return ModelBuilder;
        }

        public void SetModelBuilder(ModelBuilder modelBuilder)
        {
            ModelBuilder = modelBuilder;
        }

        public abstract void Map();
    }
}

using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace FluentHelper.EntityFrameworkCore.Common
{
    public abstract class EfDbMap<T> : IDbMap<T> where T : class
    {
        private ModelBuilder? _modelBuilder;

        public EntityTypeBuilder<T> Entity => GetModelBuilder().Entity<T>();

        public ModelBuilder GetModelBuilder()
        {
            if (_modelBuilder == null)
                throw new ArgumentNullException("ModelBuilder has not been set");

            return _modelBuilder;
        }

        public void SetModelBuilder(ModelBuilder modelBuilder)
        {
            _modelBuilder = modelBuilder;
        }

        public Type GetMappedType()
        {
            return typeof(T);
        }

        public abstract void Map();
    }
}

using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace FluentHelper.EntityFrameworkCore.Common
{
    /// <summary>
    /// An abstract class representing the mapping of a specific T model. Map method should be extended and mapping applied to 'Entity' property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EfDbMap<T> : IDbMap<T> where T : class
    {
        private ModelBuilder? _modelBuilder;

        public EntityTypeBuilder<T> Entity => GetModelBuilder().Entity<T>();

        public ModelBuilder GetModelBuilder()
        {
            if (_modelBuilder == null)
                throw new InvalidOperationException("ModelBuilder has not been set");

            return _modelBuilder;
        }

        public void SetModelBuilder(ModelBuilder modelBuilder)
        {
            if (_modelBuilder != null)
                throw new InvalidOperationException("ModelBuilder cannot be set twice");

            _modelBuilder = modelBuilder;
        }

        public Type GetMappedType()
        {
            return typeof(T);
        }

        public abstract void Map();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FluentHelper.EntityFrameworkCore.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace FluentHelper.EntityFrameworkCore.Interfaces
{
    internal interface IDbMap
    {
        ModelBuilder GetModelBuilder();

        void SetModelBuilder(ModelBuilder modelBuilder);

        Type GetMappedType();

        void Map();
    }

    internal interface IDbMap<T> : IDbMap where T : class
    {
        EntityTypeBuilder<T> Entity { get; }
    }
}

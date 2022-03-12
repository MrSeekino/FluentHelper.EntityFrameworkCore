using FluentHelper.EntityFrameworkCore.Common;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FluentHelper.EntityFrameworkCore.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace FluentHelper.EntityFrameworkCore.Interfaces
{
    internal interface IDbMap
    {
        ModelBuilder GetModelBuilder();

        void SetModelBuilder(ModelBuilder modelBuilder);

        void Map();
    }
}

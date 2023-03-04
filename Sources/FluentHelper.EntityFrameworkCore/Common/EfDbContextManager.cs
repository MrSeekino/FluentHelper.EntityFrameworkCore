using FluentHelper.EntityFrameworkCore.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace FluentHelper.EntityFrameworkCore.Common
{
    [ExcludeFromCodeCoverage]
    public class EfDbContextManager
    {
        public static IDbContext GenerateContext()
        {
            return new EfDbContext();
        }
    }
}

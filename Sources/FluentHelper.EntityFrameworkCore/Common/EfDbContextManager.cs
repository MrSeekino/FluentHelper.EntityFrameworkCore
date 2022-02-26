using FluentHelper.EntityFrameworkCore.Interfaces;

namespace FluentHelper.EntityFrameworkCore.Common
{
    public class EfDbContextManager
    {
        public static IDbContext GenerateContext()
        {
            return new EfDbContext();
        }
    }
}

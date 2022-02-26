using EntityFramework.FluentHelperCore.Interfaces;

namespace EntityFramework.FluentHelperCore.Common
{
    public class EfDbContextManager
    {
        public static IDbContext GenerateContext()
        {
            return new EfDbContext();
        }
    }
}

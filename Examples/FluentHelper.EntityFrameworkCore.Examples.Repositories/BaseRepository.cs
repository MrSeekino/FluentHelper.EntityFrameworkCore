using EntityFramework.FluentHelperCore.Examples.Dao;
using EntityFramework.FluentHelperCore.Interfaces;

namespace EntityFramework.FluentHelperCore.Examples.Repositories
{
    public class BaseRepository
    {
        protected IDbContext DbContext { get; set; }

        public BaseRepository()
        {
            DbContext = DaoInitializer.InitializeContext();
        }

        public BaseRepository(IDbContext dbContext)
        {
            DbContext = dbContext;
        }
    }
}

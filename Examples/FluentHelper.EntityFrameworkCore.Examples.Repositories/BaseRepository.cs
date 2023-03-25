using FluentHelper.EntityFrameworkCore.Interfaces;

namespace FluentHelper.EntityFrameworkCore.Examples.Repositories
{
    public class BaseRepository
    {
        protected IDbContext DbContext { get; set; }

        public BaseRepository(IDbContext dbContext)
        {
            DbContext = dbContext;
        }
    }
}

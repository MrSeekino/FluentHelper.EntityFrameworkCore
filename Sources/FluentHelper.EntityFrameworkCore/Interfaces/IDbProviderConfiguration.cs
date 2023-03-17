using Microsoft.EntityFrameworkCore;

namespace FluentHelper.EntityFrameworkCore.Interfaces
{
    public interface IDbProviderConfiguration
    {
        DbContextOptionsBuilder ConfigureDbProvider(DbContextOptionsBuilder dbContextOptionsBuilder);
    }
}

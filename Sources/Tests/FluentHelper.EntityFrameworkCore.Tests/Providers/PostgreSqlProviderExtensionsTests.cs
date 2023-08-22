using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using FluentHelper.EntityFrameworkCore.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;

namespace FluentHelper.EntityFrameworkCore.Tests.Providers
{
    [TestFixture]
    internal class PostgreSqlProviderExtensionsTests
    {
        [Test]
        public void Verify_WithSqlDbProvider_WorksCorrectly()
        {
            Mock<DbContextOptions> mockedDbContextOptions = new Mock<DbContextOptions>();

            Mock<DbContextOptionsBuilder> mockedContextOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockedContextOptionsBuilder.Setup(x => x.Options).Returns(mockedDbContextOptions.Object);

            EfDbConfigBuilder efDbConfigBuilder = new EfDbConfigBuilder();
            efDbConfigBuilder.WithPostgreSqlProvider("A_Connection_String");

            Assert.DoesNotThrow(() => efDbConfigBuilder.DbProvider!(mockedContextOptionsBuilder.Object));
        }

        [Test]
        public void Verify_WithSqlDbProvider_WorksCorrectly_WithMoreOptions()
        {
            Mock<DbContextOptions> mockedDbContextOptions = new Mock<DbContextOptions>();

            Mock<DbContextOptionsBuilder> mockedContextOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockedContextOptionsBuilder.Setup(x => x.Options).Returns(mockedDbContextOptions.Object);

            EfDbConfigBuilder efDbConfigBuilder = new EfDbConfigBuilder();
            efDbConfigBuilder.WithPostgreSqlProvider("A_Connection_String", x => x.MinBatchSize(1));

            Assert.DoesNotThrow(() => efDbConfigBuilder.DbProvider!(mockedContextOptionsBuilder.Object));
        }

        [Test]
        public void Verify_WithSqlDbProvider_Throws_WhenConnectionString_IsEmpty()
        {
            EfDbConfigBuilder efDbConfigBuilder = new EfDbConfigBuilder();
            Assert.Throws<ArgumentNullException>(() => efDbConfigBuilder.WithPostgreSqlProvider(string.Empty));
        }

        [Test]
        public void Verify_BeginTransaction_With_IsolationLevel_Throws_WhenTransaction_IsOpen()
        {
            var dbContextTransaction = new Mock<IDbContextTransaction>();
            var sqlDbContextMock = new Mock<DbContext>();
            var dbMock = new Mock<DatabaseFacade>(sqlDbContextMock.Object);
            dbMock.Setup(x => x.CurrentTransaction).Returns(dbContextTransaction.Object);

            var mockedDbModel = new Mock<EfDbModel>(It.IsAny<IDbConfig>(), It.IsAny<IEnumerable<IDbMap>>());
            mockedDbModel.Setup(x => x.Database).Returns(dbMock.Object);

            var mockedDbConfig = new Mock<IDbConfig>();

            EfDbContext dbContext = new EfDbContext(mockedDbConfig.Object, new List<IDbMap>(), (c, m) => mockedDbModel.Object);
            Assert.Throws<InvalidOperationException>(() => dbContext.BeginTransaction(IsolationLevel.ReadUncommitted));
        }
    }
}

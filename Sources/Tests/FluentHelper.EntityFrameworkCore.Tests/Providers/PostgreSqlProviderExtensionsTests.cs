using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using FluentHelper.EntityFrameworkCore.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using NSubstitute;
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
            var contextOptBuilder = new DbContextOptionsBuilder();

            EfDbConfigBuilder efDbConfigBuilder = new EfDbConfigBuilder();
            efDbConfigBuilder.WithPostgreSqlProvider("A_Connection_String");

            Assert.DoesNotThrow(() => efDbConfigBuilder.DbProvider!(contextOptBuilder));
        }

        [Test]
        public void Verify_WithSqlDbProvider_WorksCorrectly_WithMoreOptions()
        {
            var contextOptBuilder = new DbContextOptionsBuilder();

            EfDbConfigBuilder efDbConfigBuilder = new EfDbConfigBuilder();
            efDbConfigBuilder.WithPostgreSqlProvider("A_Connection_String", x => x.MinBatchSize(1));

            Assert.DoesNotThrow(() => efDbConfigBuilder.DbProvider!(contextOptBuilder));
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
            var dbConfig = Substitute.For<IDbConfig>();

            var contextTransaction = Substitute.For<IDbContextTransaction>();
            var dbContext = Substitute.For<DbContext>();

            var dbFacade = Substitute.For<DatabaseFacade>(dbContext);
            dbFacade.CurrentTransaction.Returns(contextTransaction);

            var dbModel = Substitute.For<EfDbModel>(dbConfig, new List<IDbMap>());
            dbModel.Database.Returns(dbFacade);

            EfDbContext realDbContext = new EfDbContext(dbConfig, new List<IDbMap>(), (c, m) => dbModel);
            Assert.Throws<InvalidOperationException>(() => realDbContext.BeginTransaction(IsolationLevel.ReadUncommitted));
        }
    }
}

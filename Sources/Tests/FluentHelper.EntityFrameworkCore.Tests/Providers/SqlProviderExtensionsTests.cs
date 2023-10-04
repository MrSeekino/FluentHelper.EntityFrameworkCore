﻿using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using FluentHelper.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHelper.EntityFrameworkCore.Tests.Providers
{
    [TestFixture]
    public class SqlProviderExtensionsTests
    {
        [Test]
        public void Verify_WithSqlDbProvider_WorksCorrectly()
        {
            var contextOptBuilder = new DbContextOptionsBuilder();

            EfDbConfigBuilder efDbConfigBuilder = new EfDbConfigBuilder();
            efDbConfigBuilder.WithSqlDbProvider("A_Connection_String");

            Assert.DoesNotThrow(() => efDbConfigBuilder.DbProvider!(contextOptBuilder));
        }

        [Test]
        public void Verify_WithSqlDbProvider_WorksCorrectly_WithMoreOptions()
        {
            var contextOptBuilder = new DbContextOptionsBuilder();

            EfDbConfigBuilder efDbConfigBuilder = new EfDbConfigBuilder();
            efDbConfigBuilder.WithSqlDbProvider("A_Connection_String", x => x.MinBatchSize(1));

            Assert.DoesNotThrow(() => efDbConfigBuilder.DbProvider!(contextOptBuilder));
        }

        [Test]
        public void Verify_WithSqlDbProvider_Throws_WhenConnectionString_IsEmpty()
        {
            EfDbConfigBuilder efDbConfigBuilder = new EfDbConfigBuilder();
            Assert.Throws<ArgumentNullException>(() => efDbConfigBuilder.WithSqlDbProvider(string.Empty));
        }

        [Test]
        public void Verify_SqlProviderContext_ReturnCorrectProviderName()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddFluentDbContext(efDbConfigBuilder =>
            {
                efDbConfigBuilder.WithSqlDbProvider("A_Connection_String");
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var dbContext = serviceProvider.GetRequiredService<IDbContext>();

            Assert.IsNotNull(dbContext);
            Assert.AreEqual("Microsoft.EntityFrameworkCore.SqlServer", dbContext.GetProviderName());
        }

        [Test]
        public void Verify_BeginTransaction_With_IsolationLevel_Works()
        {
            var dbConfig = Substitute.For<IDbConfig>();

            var contextTransaction = Substitute.For<IDbContextTransaction>();
            var dbContext = Substitute.For<DbContext>();

            var dbFacade = Substitute.For<DatabaseFacade, IDatabaseFacadeDependenciesAccessor>(dbContext);
            dbFacade.CurrentTransaction.Returns(x => null);

            var facadeDependencies = Substitute.For<IDatabaseFacadeDependencies>();
            var executionStrategy = Substitute.For<IExecutionStrategy>();

            ((IDatabaseFacadeDependenciesAccessor)dbFacade).Dependencies.Returns(facadeDependencies);
            facadeDependencies.ExecutionStrategy.Returns(executionStrategy);
            executionStrategy.Execute(dbFacade, Arg.Any<Func<DbContext, DatabaseFacade, IDbContextTransaction>>(), null).Returns(contextTransaction);

            var dbModel = Substitute.For<EfDbModel>(dbConfig, new List<IDbMap>());
            dbModel.Database.Returns(dbFacade);

            EfDbContext realDbContext = new EfDbContext(dbConfig, new List<IDbMap>(), (c, m) => dbModel);
            var transaction = realDbContext.BeginTransaction(IsolationLevel.ReadUncommitted);

            Assert.NotNull(transaction);
            Assert.AreEqual(contextTransaction, transaction);
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

        [Test]
        public async Task Verify_BeginTransactionAsync_With_IsolationLevel_Works()
        {
            var dbConfig = Substitute.For<IDbConfig>();

            var contextTransaction = Substitute.For<IDbContextTransaction>();
            var dbContext = Substitute.For<DbContext>();

            var dbFacade = Substitute.For<DatabaseFacade, IDatabaseFacadeDependenciesAccessor>(dbContext);
            dbFacade.CurrentTransaction.Returns(x => null);

            var facadeDependencies = Substitute.For<IDatabaseFacadeDependencies>();
            var executionStrategy = Substitute.For<IExecutionStrategy>();

            ((IDatabaseFacadeDependenciesAccessor)dbFacade).Dependencies.Returns(facadeDependencies);
            facadeDependencies.ExecutionStrategy.Returns(executionStrategy);
            executionStrategy.ExecuteAsync(dbFacade, Arg.Any<Func<DbContext, DatabaseFacade, CancellationToken, Task<IDbContextTransaction>>>(), null).Returns(Task.FromResult(contextTransaction));

            var dbModel = Substitute.For<EfDbModel>(dbConfig, new List<IDbMap>());
            dbModel.Database.Returns(dbFacade);

            EfDbContext realDbContext = new EfDbContext(dbConfig, new List<IDbMap>(), (c, m) => dbModel);
            var transaction = await realDbContext.BeginTransactionAsync(IsolationLevel.ReadUncommitted);

            Assert.NotNull(transaction);
            Assert.AreEqual(contextTransaction, transaction);
        }

        [Test]
        public void Verify_BeginTransactionAsync_With_IsolationLevel_Throws_WhenTransaction_IsOpen()
        {
            var dbConfig = Substitute.For<IDbConfig>();

            var contextTransaction = Substitute.For<IDbContextTransaction>();
            var dbContext = Substitute.For<DbContext>();

            var dbFacade = Substitute.For<DatabaseFacade>(dbContext);
            dbFacade.CurrentTransaction.Returns(contextTransaction);

            var dbModel = Substitute.For<EfDbModel>(dbConfig, new List<IDbMap>());
            dbModel.Database.Returns(dbFacade);

            EfDbContext realDbContext = new EfDbContext(dbConfig, new List<IDbMap>(), (c, m) => dbModel);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await realDbContext.BeginTransactionAsync(IsolationLevel.ReadUncommitted));
        }
    }
}

﻿using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using FluentHelper.EntityFrameworkCore.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace FluentHelper.EntityFrameworkCore.Tests.Core
{
    [TestFixture]
    public class EfDbContextTests
    {
        [Test]
        public void Verify_EfDbContext_IsCreatedCorrectly()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();
            var dbMap = Substitute.For<IDbMap>();

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>() { dbMap });
            var underlyingContext = dbContext.CreateNewContext();

            Assert.That(underlyingContext, Is.Not.Null);
            Assert.That(underlyingContext.GetType(), Is.EqualTo(typeof(EfDbModel)));
            Assert.That(((EfDbModel)underlyingContext).MappingsLength, Is.EqualTo(1));
        }

        [Test]
        public void Verify_CreateDbContext_IsCalledCorrectly()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();
            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());

            bool funcCalled = false;
            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                funcCalled = true;
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            ClassicAssert.True(funcCalled);

            dbContext.CreateNewContext();
            dbModel.Received(1).Dispose();

            dbContext.CreateNewContext();
            dbModel.Received(2).Dispose();
        }

        [Test]
        public void Verify_CreateDbContext_WorksProperly_AfterSetAllProperties_WithLazyLoading()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();

            Action<DbContextOptionsBuilder> lazyLoadingProxyBehaviour = x => x.UseLazyLoadingProxies();

            var dbConfig = Substitute.For<IDbConfig>();
            dbConfig.DbConfiguration.Returns(null);
            dbConfig.DbProvider.Returns(x => { });
            dbConfig.LogAction.Returns((x, y, z) => { });
            dbConfig.EnableSensitiveDataLogging.Returns(true);
            dbConfig.LazyLoadingProxiesBehaviour.Returns(lazyLoadingProxyBehaviour);
            dbConfig.MappingAssemblies.Returns(new List<Assembly>());

            var dbMap = Substitute.For<IDbMap>();

            bool funcCalledCorrecly = false;
            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                if (c.DbProvider == dbConfig.DbProvider
                        && c.LogAction == dbConfig.LogAction
                        && c.EnableSensitiveDataLogging == dbConfig.EnableSensitiveDataLogging
                        && c.LazyLoadingProxiesBehaviour == lazyLoadingProxyBehaviour)
                    funcCalledCorrecly = true;

                return new EfDbModel(l, c, m);
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>() { dbMap }, createDbContextBehaviour);
            var underlyingContext = dbContext.CreateNewContext();

            ClassicAssert.True(funcCalledCorrecly);
            Assert.That(underlyingContext, Is.Not.Null);
            Assert.That(underlyingContext.GetType(), Is.EqualTo(typeof(EfDbModel)));
            Assert.That(((EfDbModel)underlyingContext).MappingsLength, Is.EqualTo(1));
        }

        [Test]
        public void Verify_GetContext_WorksProperly()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();
            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());

            bool funcCalled = false;
            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                funcCalled = true;
                return dbModel;
            };

            var dbContext = new EfDbContext(dbModel, loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);

            var contextGot = dbContext.GetContext();

            ClassicAssert.False(funcCalled);

            Assert.That(contextGot, Is.Not.Null);
            Assert.That(contextGot, Is.EqualTo(dbModel));

            dbModel.Received(0).Dispose();
            ClassicAssert.False(funcCalled);
        }

        [Test]
        public void Verify_Transactions_WorksProperly()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();

            var sqlDbContext = Substitute.For<DbContext>();
            var contextTransaction = Substitute.For<IDbContextTransaction>();

            var dbFacade = Substitute.For<DatabaseFacade>(sqlDbContext);
            dbFacade.CurrentTransaction.Returns((IDbContextTransaction?)null);

            contextTransaction.When(x => x.Commit()).Do(x =>
            {
                dbFacade.CurrentTransaction.Returns((IDbContextTransaction?)null);
            });
            contextTransaction.When(x => x.Rollback()).Do(x =>
            {
                dbFacade.CurrentTransaction.Returns((IDbContextTransaction?)null);
            });
            dbFacade.When(x => x.BeginTransaction()).Do(x =>
            {
                dbFacade.CurrentTransaction.Returns(contextTransaction);
            });

            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());
            dbModel.Database.Returns(dbFacade);

            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            dbContext.BeginTransaction();
            dbFacade.Received(1).BeginTransaction();

            Assert.Throws<InvalidOperationException>(() => { dbContext.BeginTransaction(); });

            dbContext.CommitTransaction();
            contextTransaction.Received(1).Commit();

            dbContext.BeginTransaction();
            dbFacade.Received(2).BeginTransaction();

            dbContext.RollbackTransaction();
            contextTransaction.Received(1).Rollback();

            Assert.Throws<InvalidOperationException>(() => { dbContext.RollbackTransaction(); });
            Assert.Throws<InvalidOperationException>(() => { dbContext.CommitTransaction(); });
        }

        [Test]
        public async Task Verify_TransactionsAsync_WorksProperly()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();

            var sqlDbContext = Substitute.For<DbContext>();
            var contextTransaction = Substitute.For<IDbContextTransaction>();

            var dbFacade = Substitute.For<DatabaseFacade>(sqlDbContext);
            dbFacade.CurrentTransaction.Returns((IDbContextTransaction?)null);

            contextTransaction.When(x => x.CommitAsync()).Do(x =>
            {
                dbFacade.CurrentTransaction.Returns((IDbContextTransaction?)null);
            });
            contextTransaction.When(x => x.RollbackAsync()).Do(x =>
            {
                dbFacade.CurrentTransaction.Returns((IDbContextTransaction?)null);
            });
            dbFacade.When(x => x.BeginTransactionAsync()).Do(x =>
            {
                dbFacade.CurrentTransaction.Returns(contextTransaction);
            });

            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());
            dbModel.Database.Returns(dbFacade);

            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            await dbContext.BeginTransactionAsync();
            await dbFacade.Received(1).BeginTransactionAsync();

            Assert.ThrowsAsync<InvalidOperationException>(async () => { await dbContext.BeginTransactionAsync(); });

            await dbContext.CommitTransactionAsync();
            await contextTransaction.Received(1).CommitAsync();

            await dbContext.BeginTransactionAsync();
            await dbFacade.Received(2).BeginTransactionAsync();

            await dbContext.RollbackTransactionAsync();
            await contextTransaction.Received(1).RollbackAsync();

            Assert.ThrowsAsync<InvalidOperationException>(async () => await dbContext.RollbackTransactionAsync());
            Assert.ThrowsAsync<InvalidOperationException>(async () => await dbContext.CommitTransactionAsync());
        }

        [Test]
        public void Verify_SavePoints_WorksProperly()
        {
            string savePointName = "A_SavePoint";

            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();

            var sqlDbContext = Substitute.For<DbContext>();
            var contextTransaction = Substitute.For<IDbContextTransaction>();
            var dbFacade = Substitute.For<DatabaseFacade>(sqlDbContext);

            contextTransaction.SupportsSavepoints.Returns(true);
            dbFacade.CurrentTransaction.Returns(contextTransaction);
            contextTransaction.When(x => x.Commit()).Do(x =>
            {
                dbFacade.CurrentTransaction.Returns((IDbContextTransaction?)null);
            });

            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());
            dbModel.Database.Returns(dbFacade);

            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            dbContext.AreSavepointsSupported();
            _ = contextTransaction.Received(1).SupportsSavepoints;

            dbContext.CreateSavepoint(savePointName);
            contextTransaction.Received(1).CreateSavepoint(savePointName);

            dbContext.ReleaseSavepoint(savePointName);
            contextTransaction.Received(1).ReleaseSavepoint(savePointName);

            dbContext.RollbackToSavepoint(savePointName);
            contextTransaction.Received(1).RollbackToSavepoint(savePointName);

            dbContext.CommitTransaction();

            Assert.Throws<InvalidOperationException>(() => { dbContext.AreSavepointsSupported(); });
            Assert.Throws<InvalidOperationException>(() => { dbContext.CreateSavepoint(savePointName); });
            Assert.Throws<InvalidOperationException>(() => { dbContext.ReleaseSavepoint(savePointName); });
            Assert.Throws<InvalidOperationException>(() => { dbContext.RollbackToSavepoint(savePointName); });
        }

        [Test]
        public async Task Verify_SavePointsAsync_WorksProperly()
        {
            string savePointName = "A_SavePoint";

            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();

            var sqlDbContext = Substitute.For<DbContext>();
            var contextTransaction = Substitute.For<IDbContextTransaction>();
            var dbFacade = Substitute.For<DatabaseFacade>(sqlDbContext);

            contextTransaction.SupportsSavepoints.Returns(true);
            dbFacade.CurrentTransaction.Returns(contextTransaction);
            contextTransaction.When(x => x.CommitAsync()).Do(x =>
            {
                dbFacade.CurrentTransaction.Returns((IDbContextTransaction?)null);
            });

            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());
            dbModel.Database.Returns(dbFacade);

            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            dbContext.AreSavepointsSupported();
            _ = contextTransaction.Received(1).SupportsSavepoints;

            await dbContext.CreateSavepointAsync(savePointName);
            await contextTransaction.Received(1).CreateSavepointAsync(savePointName);

            await dbContext.ReleaseSavepointAsync(savePointName);
            await contextTransaction.Received(1).ReleaseSavepointAsync(savePointName);

            await dbContext.RollbackToSavepointAsync(savePointName);
            await contextTransaction.Received(1).RollbackToSavepointAsync(savePointName);

            await dbContext.CommitTransactionAsync();

            Assert.Throws<InvalidOperationException>(() => dbContext.AreSavepointsSupported());
            Assert.ThrowsAsync<InvalidOperationException>(async () => await dbContext.CreateSavepointAsync(savePointName));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await dbContext.ReleaseSavepointAsync(savePointName));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await dbContext.RollbackToSavepointAsync(savePointName));
        }

        [Test]
        public void Verify_QueriesMethods_WorksProperly()
        {
            var testDataList = new List<TestEntity>() { new TestEntity() };

            int setCalls = 0;

            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();
            var queryProvider = Substitute.For<IQueryProvider>();

            var dbSet = Substitute.For<DbSet<TestEntity>, IQueryable<TestEntity>>();
            ((IQueryable<TestEntity>)dbSet).Provider.Returns(queryProvider);
            ((IQueryable<TestEntity>)dbSet).Expression.Returns(Expression.Constant(testDataList.AsQueryable()));

            var sqlDbContext = Substitute.For<DbContext>();
            var dbFacade = Substitute.For<DatabaseFacade>(sqlDbContext);
            dbFacade.CanConnect().Returns(true);

            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());
            dbModel.Set<TestEntity>().Returns(dbSet);
            dbModel.Database.Returns(dbFacade);

            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            var queryable = dbContext.Query<TestEntity>();
            setCalls++;

            dbModel.Received(setCalls).Set<TestEntity>();
            dbSet.Received(1).AsQueryable();

            dbContext.Add(new TestEntity());
            setCalls++;

            dbModel.Received(setCalls).Set<TestEntity>();
            dbSet.Received(1).Add(Arg.Any<TestEntity>());

            dbContext.AddRange(new List<TestEntity>());
            setCalls++;

            dbModel.Received(setCalls).Set<TestEntity>();
            dbSet.Received(1).AddRange(Arg.Any<List<TestEntity>>());

            dbContext.Remove(new TestEntity());
            setCalls++;

            dbModel.Received(setCalls).Set<TestEntity>();
            dbSet.Received(1).Remove(Arg.Any<TestEntity>());

            dbContext.RemoveRange(new List<TestEntity>());
            setCalls++;

            dbModel.Received(setCalls).Set<TestEntity>();
            dbSet.Received(1).RemoveRange(Arg.Any<List<TestEntity>>());
        }

        [Test]
        public async Task Verify_AddsMethodsAsync_WorksProperly()
        {
            var testDataList = new List<TestEntity>() { new TestEntity() };

            int setCalls = 0;

            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();
            var queryProvider = Substitute.For<IQueryProvider>();

            var dbSet = Substitute.For<DbSet<TestEntity>, IQueryable<TestEntity>>();
            ((IQueryable<TestEntity>)dbSet).Provider.Returns(queryProvider);
            ((IQueryable<TestEntity>)dbSet).Expression.Returns(Expression.Constant(testDataList.AsQueryable()));

            var sqlDbContext = Substitute.For<DbContext>();
            var dbFacade = Substitute.For<DatabaseFacade>(sqlDbContext);
            dbFacade.CanConnect().Returns(true);

            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());
            dbModel.Set<TestEntity>().Returns(dbSet);
            dbModel.Database.Returns(dbFacade);

            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            await dbContext.AddAsync(new TestEntity());
            setCalls++;

            dbModel.Received(setCalls).Set<TestEntity>();
            await dbSet.Received(1).AddAsync(Arg.Any<TestEntity>());

            await dbContext.AddRangeAsync(new List<TestEntity>());
            setCalls++;

            dbModel.Received(setCalls).Set<TestEntity>();
            await dbSet.Received(1).AddRangeAsync(Arg.Any<List<TestEntity>>());
        }

        [Test]
        public void Verify_SaveChanges_WorksProperly()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();

            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());
            dbModel.SaveChanges().Returns(0);

            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            int result = dbContext.SaveChanges();
            dbModel.Received(1).SaveChanges();
        }

        [Test]
        public async Task Verify_SaveChangesAsync_WorksProperly()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();

            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());
            dbModel.SaveChanges().Returns(0);

            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            int result = await dbContext.SaveChangesAsync();
            await dbModel.Received(1).SaveChangesAsync();
        }

        [Test]
        public void Verify_ExecuteOnDatabase_WithReturn_WorksProperly()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();

            var sqlDbContext = Substitute.For<DbContext>();
            var dbFacade = Substitute.For<DatabaseFacade>(sqlDbContext);
            dbFacade.CanConnect().Returns(true);

            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());
            dbModel.Database.Returns(dbFacade);

            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            bool opResult = dbContext.ExecuteOnDatabase(db => db.CanConnect());
            dbFacade.Received(1).CanConnect();
            ClassicAssert.True(opResult);
        }

        [Test]
        public void Verify_SetCommandTimeout_WorksProperly()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();
            var sqlDbContext = Substitute.For<DbContext>();

            var relationalConnection = Substitute.For<IRelationalConnection>();

            var facadeDependencies = Substitute.For<IRelationalDatabaseFacadeDependencies>();
            facadeDependencies.RelationalConnection.Returns(relationalConnection);

            var dbFacade = Substitute.For<DatabaseFacade, IDatabaseFacadeDependenciesAccessor>(sqlDbContext);
            ((IDatabaseFacadeDependenciesAccessor)dbFacade).Dependencies.Returns(facadeDependencies);

            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());
            dbModel.Database.Returns(dbFacade);

            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            dbContext.SetCommandTimeout(TimeSpan.FromMinutes(10));
            var currentTimeout = relationalConnection!.CommandTimeout!.Value;
            ClassicAssert.AreEqual(600, currentTimeout);
        }

        [Test]
        public void Verify_CanConnect_WorksProperly()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();

            var sqlDbContext = Substitute.For<DbContext>();
            var dbFacade = Substitute.For<DatabaseFacade>(sqlDbContext);
            dbFacade.CanConnect().Returns(true);

            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());
            dbModel.Database.Returns(dbFacade);

            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            bool opResult = dbContext.CanConnect();
            dbFacade.Received(1).CanConnect();
            ClassicAssert.True(opResult);
        }

        [Test]
        public async Task Verify_CanConnectAsync_WorksProperly()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();

            var sqlDbContext = Substitute.For<DbContext>();
            var dbFacade = Substitute.For<DatabaseFacade>(sqlDbContext);
            dbFacade.CanConnectAsync().Returns(true);

            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());
            dbModel.Database.Returns(dbFacade);

            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            bool opResult = await dbContext.CanConnectAsync();
            await dbFacade.Received(1).CanConnectAsync();
            ClassicAssert.True(opResult);
        }

        [Test]
        public void Verify_GetConnectionString_WorksProperly()
        {
            string connectionString = "The_Connection_String";

            var loggerFactory = Substitute.For<ILoggerFactory>();
            var dbConfig = Substitute.For<IDbConfig>();
            var sqlDbContext = Substitute.For<DbContext>();

            var relationalConnection = Substitute.For<IRelationalConnection>();

            var facadeDependencies = Substitute.For<IRelationalDatabaseFacadeDependencies>();
            facadeDependencies.RelationalConnection.Returns(relationalConnection);

            var dbFacade = Substitute.For<DatabaseFacade, IDatabaseFacadeDependenciesAccessor>(sqlDbContext);
            ((IDatabaseFacadeDependenciesAccessor)dbFacade).Dependencies.Returns(facadeDependencies);
            dbFacade.GetConnectionString().Returns(connectionString);

            var dbModel = Substitute.For<EfDbModel>(loggerFactory, dbConfig, new List<IDbMap>());
            dbModel.Database.Returns(dbFacade);

            Func<ILoggerFactory, IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (l, c, m) =>
            {
                return dbModel;
            };

            var dbContext = new EfDbContext(loggerFactory, dbConfig, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateNewContext();

            var connString = dbContext.GetConnectionString();
            ClassicAssert.AreEqual(connectionString, connString);
        }
    }
}

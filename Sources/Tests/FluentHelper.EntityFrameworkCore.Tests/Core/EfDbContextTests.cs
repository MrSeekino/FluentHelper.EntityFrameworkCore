using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using FluentHelper.EntityFrameworkCore.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FluentHelper.EntityFrameworkCore.Tests.Core
{
    [TestFixture]
    internal class EfDbContextTests
    {
        [Test]
        public void Verify_EfDbContext_IsCreatedCorrectly()
        {
            var dbConfigMock = new Mock<IDbConfig>();
            var dbMapMock = new Mock<IDbMap>();

            var dbContext = new EfDbContext(dbConfigMock.Object, new List<IDbMap>() { dbMapMock.Object });
            Assert.That(dbContext.CreateDbContextBehaviour, Is.Not.Null);

            dbContext.CreateDbContext();
            Assert.That(dbContext.DbContext, Is.Not.Null);
            Assert.That(dbContext.DbContext!.GetType(), Is.EqualTo(typeof(EfDbModel)));
            Assert.That(((EfDbModel)dbContext.DbContext).DbConfig, Is.Not.Null);
            Assert.That(((EfDbModel)dbContext.DbContext).Mappings.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Verify_CreateDbContext_IsCalledCorrectly()
        {
            var dbConfigMock = new Mock<IDbConfig>();
            var dbModelMock = new Mock<EfDbModel>(It.IsAny<IDbConfig>(), It.IsAny<IEnumerable<IDbMap>>());
            dbModelMock.Setup(x => x.Dispose()).Verifiable();

            bool funcCalled = false;
            Func<IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (c, m) =>
            {
                funcCalled = true;
                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(dbConfigMock.Object, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateDbContext();

            Assert.True(funcCalled);

            dbContext.CreateNewContext();
            dbModelMock.Verify(x => x.Dispose(), Times.Once());

            dbContext.CreateDbContext();
            dbModelMock.Verify(x => x.Dispose(), Times.Exactly(2));
        }

        [Test]
        public void Verify_CreateDbContext_WorksProperly_AfterSetAllProperties_WithLazyLoading()
        {
            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.Setup(x => x.DbConfiguration).Returns(null);
            dbConfigMock.Setup(x => x.DbProvider).Returns(x => { });
            dbConfigMock.Setup(x => x.LogAction).Returns((x, y, z) => { });
            dbConfigMock.Setup(x => x.EnableSensitiveDataLogging).Returns(true);
            dbConfigMock.Setup(x => x.EnableLazyLoadingProxies).Returns(true);
            dbConfigMock.Setup(x => x.MappingAssemblies).Returns(new List<System.Reflection.Assembly>());

            var mockDbMap = new Mock<IDbMap>();

            bool funcCalledCorrecly = false;
            Func<IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (c, m) =>
            {
                if (c.DbProvider == dbConfigMock.Object.DbProvider
                        && c.LogAction == dbConfigMock.Object.LogAction
                        && c.EnableSensitiveDataLogging == dbConfigMock.Object.EnableSensitiveDataLogging
                        && c.EnableLazyLoadingProxies == dbConfigMock.Object.EnableLazyLoadingProxies)
                    funcCalledCorrecly = true;

                return new EfDbModel(c, m);
            };

            var dbContext = new EfDbContext(dbConfigMock.Object, new List<IDbMap>() { mockDbMap.Object }, createDbContextBehaviour);
            dbContext.CreateNewContext();
            Assert.True(funcCalledCorrecly);

            Assert.That(dbContext.DbContext, Is.Not.Null);
            Assert.That(dbContext.DbContext!.GetType(), Is.EqualTo(typeof(EfDbModel)));
            Assert.That(((EfDbModel)dbContext.DbContext).Mappings.Count, Is.EqualTo(1));
        }

        [Test]
        public void Verify_GetContext_WorksProperly()
        {
            var dbConfigMock = new Mock<IDbConfig>();
            var dbModelMock = new Mock<EfDbModel>(It.IsAny<IDbConfig>(), It.IsAny<IEnumerable<IDbMap>>());

            bool funcCalled = false;
            Func<IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (c, m) =>
            {
                funcCalled = true;
                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(dbConfigMock.Object, new List<IDbMap>(), createDbContextBehaviour)
            {
                DbContext = dbModelMock.Object
            };

            var contextGot = dbContext.GetContext();

            Assert.False(funcCalled);

            Assert.That(contextGot, Is.Not.Null);
            Assert.That(contextGot, Is.EqualTo(dbModelMock.Object));

            dbModelMock.Verify(x => x.Dispose(), Times.Never());
            Assert.False(funcCalled);
        }

        [Test]
        public void Verify_Transactions_WorksProperly()
        {
            var dbConfigMock = new Mock<IDbConfig>();

            var sqlDbContextMock = new Mock<DbContext>();
            var transactionMock = new Mock<IDbContextTransaction>();
            var dbMock = new Mock<DatabaseFacade>(sqlDbContextMock.Object);

            transactionMock.Setup(x => x.Commit()).Callback(() =>
            {
                dbMock.Setup(x => x.CurrentTransaction).Returns((IDbContextTransaction?)null);
            });
            transactionMock.Setup(x => x.Rollback()).Callback(() =>
            {
                dbMock.Setup(x => x.CurrentTransaction).Returns((IDbContextTransaction?)null);
            });
            dbMock.Setup(x => x.BeginTransaction()).Callback(() =>
            {
                dbMock.Setup(x => x.CurrentTransaction).Returns(transactionMock.Object);
            });

            var dbModelMock = new Mock<EfDbModel>(It.IsAny<IDbConfig>(), It.IsAny<IEnumerable<IDbMap>>());
            dbModelMock.Setup(x => x.Database).Returns(dbMock.Object);

            Func<IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (c, m) =>
            {
                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(dbConfigMock.Object, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateDbContext();

            dbContext.BeginTransaction();
            dbMock.Verify(x => x.BeginTransaction(), Times.Once());

            Assert.Throws<InvalidOperationException>(() => { dbContext.BeginTransaction(); });

            dbContext.CommitTransaction();
            transactionMock.Verify(x => x.Commit(), Times.Once());

            dbContext.BeginTransaction();
            dbMock.Verify(x => x.BeginTransaction(), Times.Exactly(2));

            dbContext.RollbackTransaction();
            transactionMock.Verify(x => x.Rollback(), Times.Once());

            Assert.Throws<InvalidOperationException>(() => { dbContext.RollbackTransaction(); });
            Assert.Throws<InvalidOperationException>(() => { dbContext.CommitTransaction(); });
        }

        [Test]
        public void Verify_SavePoints_WorksProperly()
        {
            string savePointName = "A_SavePoint";

            var dbConfigMock = new Mock<IDbConfig>();

            var sqlDbContextMock = new Mock<DbContext>();
            var transactionMock = new Mock<IDbContextTransaction>();
            var dbMock = new Mock<DatabaseFacade>(sqlDbContextMock.Object);

            transactionMock.Setup(x => x.SupportsSavepoints).Returns(true);
            dbMock.Setup(x => x.CurrentTransaction).Returns(transactionMock.Object);
            dbMock.Setup(x => x.CurrentTransaction!.Commit()).Callback(() =>
            {
                dbMock.Setup(x => x.CurrentTransaction).Returns((IDbContextTransaction?)null);
            });

            var dbModelMock = new Mock<EfDbModel>(It.IsAny<IDbConfig>(), It.IsAny<IEnumerable<IDbMap>>());
            dbModelMock.Setup(x => x.Database).Returns(dbMock.Object);

            Func<IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (c, m) =>
            {
                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(dbConfigMock.Object, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateDbContext();

            dbContext.AreSavepointsSupported();
            transactionMock.Verify(x => x.SupportsSavepoints, Times.Once());

            dbContext.CreateSavepoint(savePointName);
            transactionMock.Verify(x => x.CreateSavepoint(savePointName), Times.Once());

            dbContext.ReleaseSavepoint(savePointName);
            transactionMock.Verify(x => x.ReleaseSavepoint(savePointName), Times.Once());

            dbContext.RollbackToSavepoint(savePointName);
            transactionMock.Verify(x => x.RollbackToSavepoint(savePointName), Times.Once());

            dbContext.CommitTransaction();

            Assert.Throws<InvalidOperationException>(() => { dbContext.AreSavepointsSupported(); });
            Assert.Throws<InvalidOperationException>(() => { dbContext.CreateSavepoint(savePointName); });
            Assert.Throws<InvalidOperationException>(() => { dbContext.ReleaseSavepoint(savePointName); });
            Assert.Throws<InvalidOperationException>(() => { dbContext.RollbackToSavepoint(savePointName); });
        }

        [Test]
        public void Verify_QueriesMethods_WorksProperly()
        {
            var testDataList = new List<TestEntity>() { new TestEntity() };

            int setCalls = 0;

            var dbConfigMock = new Mock<IDbConfig>();
            var queryProvider = new Mock<IQueryProvider>();
            queryProvider.Setup(x => x.Execute<int>(It.IsAny<Expression>())).Callback<Expression>(expression =>
            {
                Assert.That(expression, Is.Not.Null);
            });

            var dbSetMock = new Mock<DbSet<TestEntity>>();
            dbSetMock.As<IQueryable<TestEntity>>().Setup(x => x.Provider).Returns(queryProvider.Object);
            dbSetMock.As<IQueryable<TestEntity>>().Setup(x => x.Expression).Returns(Expression.Constant(testDataList.AsQueryable()));

            var dbContextOptMock = new Mock<DbContextOptions>();
            dbContextOptMock.Setup(x => x.ContextType.IsAssignableFrom(It.IsAny<Type>())).Returns(true);

            var dbFacadeMock = new Mock<DatabaseFacade>(new DbContext(dbContextOptMock.Object));
            dbFacadeMock.Setup(x => x.CanConnect()).Returns(true).Verifiable();

            var dbModelMock = new Mock<EfDbModel>(It.IsAny<IDbConfig>(), It.IsAny<IEnumerable<IDbMap>>());
            dbModelMock.Setup(x => x.Set<TestEntity>()).Returns(dbSetMock.Object);
            dbModelMock.Setup(x => x.Database).Returns(dbFacadeMock.Object);

            Func<IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (c, m) =>
            {
                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(dbConfigMock.Object, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateDbContext();

            var queryable = dbContext.Query<TestEntity>();
            setCalls++;

            dbModelMock.Verify(x => x.Set<TestEntity>(), Times.Exactly(setCalls));
            dbSetMock.Verify(x => x.AsQueryable(), Times.Once());

            dbContext.Add(new TestEntity());
            setCalls++;

            dbModelMock.Verify(x => x.Set<TestEntity>(), Times.Exactly(setCalls));
            dbSetMock.Verify(x => x.Add(It.IsAny<TestEntity>()), Times.Once());

            dbContext.AddRange(new List<TestEntity>());
            setCalls++;

            dbModelMock.Verify(x => x.Set<TestEntity>(), Times.Exactly(setCalls));
            dbSetMock.Verify(x => x.AddRange(It.IsAny<List<TestEntity>>()), Times.Once());

            dbContext.Remove(new TestEntity());
            setCalls++;

            dbModelMock.Verify(x => x.Set<TestEntity>(), Times.Exactly(setCalls));
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestEntity>()), Times.Once());

            dbContext.RemoveRange(new List<TestEntity>());
            setCalls++;

            dbModelMock.Verify(x => x.Set<TestEntity>(), Times.Exactly(setCalls));
            dbSetMock.Verify(x => x.RemoveRange(It.IsAny<List<TestEntity>>()), Times.Once());

            bool opResult = dbContext.ExecuteOnDatabase(db => db.CanConnect());
            dbFacadeMock.Verify(x => x.CanConnect(), Times.Once());
            Assert.True(opResult);
        }

        [Test]
        public void Verify_SaveChanges_WorksProperly()
        {
            var dbConfigMock = new Mock<IDbConfig>();

            var dbModelMock = new Mock<EfDbModel>(It.IsAny<IDbConfig>(), It.IsAny<IEnumerable<IDbMap>>());
            dbModelMock.Setup(x => x.SaveChanges()).Returns(0);

            Func<IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (c, m) =>
            {
                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(dbConfigMock.Object, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateDbContext();

            int result = dbContext.SaveChanges();
            dbModelMock.Verify(x => x.SaveChanges(), Times.Once());
        }
    }
}

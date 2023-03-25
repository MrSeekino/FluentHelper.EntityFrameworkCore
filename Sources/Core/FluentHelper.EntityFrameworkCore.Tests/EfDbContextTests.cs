using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using FluentHelper.EntityFrameworkCore.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentHelper.EntityFrameworkCore.Tests
{
    [TestFixture]
    internal class EfDbContextTests
    {
        [Test]
        public void Verify_CreateDbContext_IsCalledCorrectly()
        {
            var dbConfigMock = new Mock<IDbConfig>();
            var dbModelMock = new Mock<EfDbModel>(It.IsAny<IDbConfig>(), It.IsAny<IEnumerable<IDbMap>>());

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
        }

        [Test]
        public void Verify_CreateDbContext_WorksProperly_AfterSetAllProperties_WithLazyLoading()
        {
            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.Setup(x => x.DbProviderConfiguration).Returns(x => { });
            dbConfigMock.Setup(x => x.LogAction).Returns((x, y, z) => { });
            dbConfigMock.Setup(x => x.EnableSensitiveDataLogging).Returns(true);
            dbConfigMock.Setup(x => x.EnableLazyLoadingProxies).Returns(true);
            dbConfigMock.Setup(x => x.MappingAssemblies).Returns(new List<System.Reflection.Assembly>());

            var mockDbMap = new Mock<IDbMap>();

            bool funcCalledCorrecly = false;
            Func<IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (c, m) =>
            {
                if (c.DbProviderConfiguration == dbConfigMock.Object.DbProviderConfiguration
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

            Assert.Throws<Exception>(() => { dbContext.BeginTransaction(); });

            dbContext.CommitTransaction();
            transactionMock.Verify(x => x.Commit(), Times.Once());

            dbContext.BeginTransaction();
            dbMock.Verify(x => x.BeginTransaction(), Times.Exactly(2));

            dbContext.RollbackTransaction();
            transactionMock.Verify(x => x.Rollback(), Times.Once());
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

            var dbModelMock = new Mock<EfDbModel>(It.IsAny<IDbConfig>(), It.IsAny<IEnumerable<IDbMap>>());
            dbModelMock.Setup(x => x.Database).Returns(dbMock.Object);

            Func<IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (c, m) =>
            {
                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(dbConfigMock.Object, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateDbContext();

            dbContext.CreateSavepoint(savePointName);
            transactionMock.Verify(x => x.CreateSavepoint(savePointName), Times.Once());

            dbContext.ReleaseSavepoint(savePointName);
            transactionMock.Verify(x => x.ReleaseSavepoint(savePointName), Times.Once());

            dbContext.RollbackToSavepoint(savePointName);
            transactionMock.Verify(x => x.RollbackToSavepoint(savePointName), Times.Once());
        }

        [Test]
        public void Verify_QueriesMethods_WorksProperly()
        {
            int setCalls = 0;

            var dbConfigMock = new Mock<IDbConfig>();

            var dbsetMock = new Mock<DbSet<TestEntity>>();

            var dbModelMock = new Mock<EfDbModel>(It.IsAny<IDbConfig>(), It.IsAny<IEnumerable<IDbMap>>());
            dbModelMock.Setup(x => x.Set<TestEntity>()).Returns(dbsetMock.Object);

            Func<IDbConfig, IEnumerable<IDbMap>, EfDbModel> createDbContextBehaviour = (c, m) =>
            {
                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(dbConfigMock.Object, new List<IDbMap>(), createDbContextBehaviour);
            dbContext.CreateDbContext();

            var queryable = dbContext.Query<TestEntity>();
            setCalls++;

            dbModelMock.Verify(x => x.Set<TestEntity>(), Times.Exactly(setCalls));
            dbsetMock.Verify(x => x.AsQueryable(), Times.Once());

            dbContext.Add(new TestEntity());
            setCalls++;

            dbModelMock.Verify(x => x.Set<TestEntity>(), Times.Exactly(setCalls));
            dbsetMock.Verify(x => x.Add(It.IsAny<TestEntity>()), Times.Once());

            dbContext.AddRange(new List<TestEntity>());
            setCalls++;

            dbModelMock.Verify(x => x.Set<TestEntity>(), Times.Exactly(setCalls));
            dbsetMock.Verify(x => x.AddRange(It.IsAny<List<TestEntity>>()), Times.Once());

            dbContext.Remove(new TestEntity());
            setCalls++;

            dbModelMock.Verify(x => x.Set<TestEntity>(), Times.Exactly(setCalls));
            dbsetMock.Verify(x => x.Remove(It.IsAny<TestEntity>()), Times.Once());

            dbContext.RemoveRange(new List<TestEntity>());
            setCalls++;

            dbModelMock.Verify(x => x.Set<TestEntity>(), Times.Exactly(setCalls));
            dbsetMock.Verify(x => x.RemoveRange(It.IsAny<List<TestEntity>>()), Times.Once());
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

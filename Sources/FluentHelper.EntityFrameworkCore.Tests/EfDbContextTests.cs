using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FluentHelper.EntityFrameworkCore.Tests
{
    [TestFixture]
    internal class EfDbContextTests
    {
        [Test]
        public void Verify_CreateDbContext_IsCalledCorrectly()
        {
            var dbModelMock = new Mock<EfDbModel>();

            bool funcCalled = false;
            Func<Action<DbContextOptionsBuilder>, Action<LogLevel, EventId, string>?, bool, bool, EfDbModel> createDbContextBehaviour = (dbc, la, sdl, llp) =>
            {
                funcCalled = true;

                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(createDbContextBehaviour);

            dbContext.CreateDbContext();
            Assert.True(funcCalled);

            dbContext.CreateNewContext();
            dbModelMock.Verify(x => x.Dispose(), Times.Once());
        }

        [Test]
        public void Verify_CreateDbContext_WorksProperly_AfterSetAllProperties_WithLazyLoading()
        {
            var dbModel = new EfDbModel();

            Action<DbContextOptionsBuilder> dbProviderConfiguration = (x) => { };
            bool enableSensitiveDataLogging = true;
            Action<LogLevel, EventId, string> logAction = (x, y, z) => { };
            Func<EventId, LogLevel, bool> logFilter = (x, y) => { return true; };

            bool funcCalledCorrecly = false;
            Func<Action<DbContextOptionsBuilder>, Action<LogLevel, EventId, string>?, bool, bool, EfDbModel> createDbContextBehaviour = (dbc, la, sdl, llp) =>
            {
                if (dbc == dbProviderConfiguration && la == logAction & sdl == enableSensitiveDataLogging
                        && llp == true)
                    funcCalledCorrecly = true;

                return dbModel;
            };

            var dbContext = new EfDbContext(createDbContextBehaviour)
                                .WithDbProviderConfiguration(dbProviderConfiguration)
                                .WithLogAction(logAction, enableSensitiveDataLogging)
                                .WithLazyLoadingProxies();

            dbContext.CreateNewContext();
            Assert.True(funcCalledCorrecly);

            dbContext.WithMappingFromAssemblyOf<TestEntityMap>();
            Assert.That(dbModel.MappingAssemblies.Count, Is.EqualTo(1));
        }

        [Test]
        public void Verify_CreateDbContext_WorksProperly_AfterSetAllProperties_WithoutLazyLoading()
        {
            var dbModel = new EfDbModel();

            Action<DbContextOptionsBuilder> dbProviderConfiguration = (x) => { };
            bool enableSensitiveDataLogging = true;
            Action<LogLevel, EventId, string> logAction = (x, y, z) => { };
            Func<EventId, LogLevel, bool> logFilter = (x, y) => { return true; };

            bool funcCalledCorrecly = false;
            Func<Action<DbContextOptionsBuilder>, Action<LogLevel, EventId, string>?, bool, bool, EfDbModel> createDbContextBehaviour = (dbc, la, sdl, llp) =>
            {
                if (dbc == dbProviderConfiguration && la == logAction & sdl == enableSensitiveDataLogging
                        && llp == true)
                    funcCalledCorrecly = true;

                return dbModel;
            };

            var dbContext = new EfDbContext(createDbContextBehaviour)
                                .WithDbProviderConfiguration(dbProviderConfiguration)
                                .WithLogAction(logAction, enableSensitiveDataLogging);

            dbContext.CreateNewContext();
            Assert.True(funcCalledCorrecly);

            dbContext.WithMappingFromAssemblyOf<TestEntityMap>();
            Assert.That(dbModel.MappingAssemblies.Count, Is.EqualTo(1));
        }

        [Test]
        public void Verify_CreateNewContext_WorksProperly()
        {
            var dbModelMock = new Mock<EfDbModel>();

            bool funcCalled = false;
            Func<Action<DbContextOptionsBuilder>, Action<LogLevel, EventId, string>?, bool, bool, EfDbModel> createDbContextBehaviour = (dbc, la, sdl, llp) =>
            {
                funcCalled = true;

                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(createDbContextBehaviour);

            dbContext.CreateNewContext();
            Assert.True(funcCalled);

            dbContext.CreateNewContext();
            dbModelMock.Verify(x => x.Dispose(), Times.Once());
        }

        [Test]
        public void Verify_GetContext_WorksProperly()
        {
            var dbModelMock = new Mock<EfDbModel>();

            bool funcCalled = false;
            Func<Action<DbContextOptionsBuilder>, Action<LogLevel, EventId, string>?, bool, bool, EfDbModel> createDbContextBehaviour = (dbc, la, sdl, llp) =>
            {
                funcCalled = true;

                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(createDbContextBehaviour)
            {
                DbContext = dbModelMock.Object
            };

            var contextGot = dbContext.GetContext();

            Assert.That(contextGot, Is.Not.Null);
            Assert.That(contextGot, Is.EqualTo(dbModelMock.Object));

            dbModelMock.Verify(x => x.Dispose(), Times.Never());
            Assert.False(funcCalled);
        }

        [Test]
        public void Verify_Transactions_WorksProperly()
        {
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

            var dbModelMock = new Mock<EfDbModel>();
            dbModelMock.Setup(x => x.Database).Returns(dbMock.Object);

            Func<Action<DbContextOptionsBuilder>, Action<LogLevel, EventId, string>?, bool, bool, EfDbModel> createDbContextBehaviour = (dbc, la, sdl, llp) =>
            {
                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(createDbContextBehaviour);
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

            var sqlDbContextMock = new Mock<DbContext>();
            var transactionMock = new Mock<IDbContextTransaction>();
            var dbMock = new Mock<DatabaseFacade>(sqlDbContextMock.Object);

            transactionMock.Setup(x => x.SupportsSavepoints).Returns(true);
            dbMock.Setup(x => x.CurrentTransaction).Returns(transactionMock.Object);

            var dbModelMock = new Mock<EfDbModel>();
            dbModelMock.Setup(x => x.Database).Returns(dbMock.Object);

            Func<Action<DbContextOptionsBuilder>, Action<LogLevel, EventId, string>?, bool, bool, EfDbModel> createDbContextBehaviour = (dbc, la, sdl, llp) =>
            {
                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(createDbContextBehaviour);
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

            var dbsetMock = new Mock<DbSet<TestEntity>>();

            var dbModelMock = new Mock<EfDbModel>();
            dbModelMock.Setup(x => x.Set<TestEntity>()).Returns(dbsetMock.Object);

            Func<Action<DbContextOptionsBuilder>, Action<LogLevel, EventId, string>?, bool, bool, EfDbModel> createDbContextBehaviour = (dbc, la, sdl, llp) =>
            {
                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(createDbContextBehaviour);
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
            var dbModelMock = new Mock<EfDbModel>();
            dbModelMock.Setup(x => x.SaveChanges()).Returns(0);

            Func<Action<DbContextOptionsBuilder>, Action<LogLevel, EventId, string>?, bool, bool, EfDbModel> createDbContextBehaviour = (dbc, la, sdl, llp) =>
            {
                return dbModelMock.Object;
            };

            var dbContext = new EfDbContext(createDbContextBehaviour);
            dbContext.CreateDbContext();

            int result = dbContext.SaveChanges();
            dbModelMock.Verify(x => x.SaveChanges(), Times.Once());
        }
    }
}

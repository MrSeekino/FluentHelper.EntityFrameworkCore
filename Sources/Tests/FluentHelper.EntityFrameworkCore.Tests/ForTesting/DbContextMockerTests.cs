//using FluentHelper.EntityFrameworkCore.Moq;
//using FluentHelper.EntityFrameworkCore.Tests.Support;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Infrastructure;
//using Microsoft.EntityFrameworkCore.Storage;
//using Moq;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace FluentHelper.EntityFrameworkCore.Tests.ForTesting
//{
//    [TestFixture]
//    internal class DbContextMockerTests
//    {
//        [Test]
//        public void Verify_DbContextMocker_IsBuildedCorrectly()
//        {
//            DbContextMocker dbContextMocker = new DbContextMocker();

//            Assert.That(dbContextMocker.MockData, Is.Not.Null);
//            Assert.That(dbContextMocker.MockData.Count, Is.EqualTo(0));
//            Assert.That(dbContextMocker.MockContext, Is.Not.Null);

//            var mockSetups = dbContextMocker.MockContext.Setups as IEnumerable<ISetup>;
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("AreSavepointsSupported()")));
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("CreateSavepoint(IsAny())")));
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("ReleaseSavepoint(IsAny())")));
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("RollbackToSavepoint(IsAny())")));
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("SaveChanges()")));
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("IsTransactionOpen()")));
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("BeginTransaction()")));
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("ExecuteOnDatabase(IsAny())")));
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("RollbackTransaction()")));
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("CommitTransaction()")));
//        }

//        [Test]
//        public void Verify_AddSupportTo_WorksCorrectly()
//        {
//            DbContextMocker dbContextMocker = new DbContextMocker();
//            dbContextMocker.AddSupportTo<TestEntity>();

//            Assert.That(dbContextMocker.MockData.Count, Is.EqualTo(1));
//            Assert.That(dbContextMocker.MockData.First().Key, Is.EqualTo(typeof(TestEntity)));

//            var mockSetups = dbContextMocker.MockContext.Setups as IEnumerable<ISetup>;
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("Query()")));
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("Add(IsAny())")));
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("AddRange(IsAny())")));
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("Remove(IsAny())")));
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("RemoveRange(IsAny())")));
//        }

//        [Test]
//        public void Verify_AddMockToExecuteOnDatabase_WorksCorrectly()
//        {
//            bool funcCalled = false;

//            var mockDbContext = new Mock<DbContext>();
//            var mockFacade = new Mock<DatabaseFacade>(mockDbContext.Object);

//            DbContextMocker dbContextMocker = new DbContextMocker();
//            dbContextMocker.AddMockToExecuteOnDatabase<bool>(mockFacade.Object, x =>
//            {
//                funcCalled = true;
//            });

//            var mockSetups = dbContextMocker.MockContext.Setups as IEnumerable<ISetup>;
//            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("ExecuteOnDatabase(IsAny())")));

//            dbContextMocker.Object.ExecuteOnDatabase(dbFacade =>
//            {
//                Assert.That(dbFacade, Is.EqualTo(mockFacade.Object));

//                return true;
//            });

//            Assert.True(funcCalled);
//        }

//        [Test]
//        public void Verify_AddMockToExecuteOnDatabase_CannotMock_IDbContextTransaction()
//        {
//            var mockDbContext = new Mock<DbContext>();
//            var mockFacade = new Mock<DatabaseFacade>(mockDbContext.Object);

//            DbContextMocker dbContextMocker = new DbContextMocker();
//            Assert.Throws<ArgumentException>(() => dbContextMocker.AddMockToExecuteOnDatabase<IDbContextTransaction>(mockFacade.Object, x => { }));
//        }
//    }
//}

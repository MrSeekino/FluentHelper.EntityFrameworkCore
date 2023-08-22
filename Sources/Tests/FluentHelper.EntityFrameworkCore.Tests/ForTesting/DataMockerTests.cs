using FluentHelper.EntityFrameworkCore.Moq;
using FluentHelper.EntityFrameworkCore.Tests.Support;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentHelper.EntityFrameworkCore.Tests.ForTesting
{
    [TestFixture]
    internal class DataMockerTests
    {
        [Test]
        public void Verify_DataMocker_IsInitializedCorrectly_Empty()
        {
            DataMocker<TestEntity> dataMocker = new DataMocker<TestEntity>();

            Assert.That(dataMocker.FinalList, Is.Not.Null);
            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.RollbackList, Is.Not.Null);
            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.AddList, Is.Not.Null);
            Assert.That(dataMocker.AddList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.RemoveList, Is.Not.Null);
            Assert.That(dataMocker.RemoveList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.HasActiveTransaction, Is.False);

            var mockSetups = dataMocker.DbContectTransactionMock.Setups as IEnumerable<ISetup>;
            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("Commit()")));
            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("Rollback()")));
        }

        [Test]
        public void Verify_DataMocker_IsInitializedCorrectly_WithData()
        {
            var initialData = new List<TestEntity>()
            {
                new TestEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Test",
                }
            };

            DataMocker<TestEntity> dataMocker = new DataMocker<TestEntity>(initialData);

            Assert.That(dataMocker.FinalList, Is.Not.Null);
            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(1));
            Assert.That(dataMocker.FinalList[0].Id, Is.EqualTo(initialData[0].Id));
            Assert.That(dataMocker.FinalList[0].Name, Is.EqualTo(initialData[0].Name));

            Assert.That(dataMocker.RollbackList, Is.Not.Null);
            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.AddList, Is.Not.Null);
            Assert.That(dataMocker.AddList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.RemoveList, Is.Not.Null);
            Assert.That(dataMocker.RemoveList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.HasActiveTransaction, Is.False);

            var mockSetups = dataMocker.DbContectTransactionMock.Setups as IEnumerable<ISetup>;
            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("Commit()")));
            Assert.True(mockSetups.Any(x => x.Expression.ToString().Contains("Rollback()")));
        }

        [Test]
        public void Verify_DataMocker_Operations_Works()
        {
            var testData = new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Test",
            };

            var testDataList = new List<TestEntity>()
            {
                new TestEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Test_1",
                },
                new TestEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Test_2",
                }
            };

            DataMocker<TestEntity> dataMocker = new DataMocker<TestEntity>();

            dataMocker.Add(testData);

            Assert.That(dataMocker.AddList.Count, Is.EqualTo(1));
            Assert.That(dataMocker.AddListCount(), Is.EqualTo(1));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(0));

            dataMocker.SaveChanges();

            Assert.That(dataMocker.AddList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.AddListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(1));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(1));

            dataMocker.Remove(testData);

            Assert.That(dataMocker.RemoveList.Count, Is.EqualTo(1));
            Assert.That(dataMocker.RemoveListCount(), Is.EqualTo(1));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(1));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(1));

            dataMocker.SaveChanges();

            Assert.That(dataMocker.RemoveList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.RemoveListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(0));

            dataMocker.AddRange(testDataList);

            Assert.That(dataMocker.AddList.Count, Is.EqualTo(2));
            Assert.That(dataMocker.AddListCount(), Is.EqualTo(2));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(0));

            dataMocker.SaveChanges();

            Assert.That(dataMocker.AddList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.AddListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(2));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(2));

            dataMocker.RemoveRange(testDataList);

            Assert.That(dataMocker.RemoveList.Count, Is.EqualTo(2));
            Assert.That(dataMocker.RemoveListCount(), Is.EqualTo(2));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(2));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(2));

            dataMocker.SaveChanges();

            Assert.That(dataMocker.RemoveList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.RemoveListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(0));
        }

        [Test]
        public void Verify_DataMocker_Throws_When_AddingSameItemTwice()
        {
            var testData = new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Test",
            };

            DataMocker<TestEntity> dataMocker = new DataMocker<TestEntity>();

            dataMocker.Add(testData);
            dataMocker.Add(testData);

            Assert.Throws<InvalidOperationException>(() => dataMocker.SaveChanges());
        }

        [Test]
        public void Verify_DataMocker_Throws_When_RemovingSameItemTwice()
        {
            var testDataList = new List<TestEntity>()
            {
                new TestEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Test_1",
                },
                new TestEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Test_2",
                }
            };

            DataMocker<TestEntity> dataMocker = new DataMocker<TestEntity>(testDataList);

            dataMocker.Remove(testDataList[0]);
            dataMocker.Remove(testDataList[0]);

            Assert.Throws<InvalidOperationException>(() => dataMocker.SaveChanges());
        }

        [Test]
        public void Verify_DataMocker_Transactions_Works()
        {
            var initialDataList = new List<TestEntity>()
            {
                new TestEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Initial",
                }
            };

            var testData = new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Test",
            };

            var testDataSecondary = new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Test_Secondary",
            };

            var testDataList = new List<TestEntity>()
            {
                new TestEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Test_1",
                },
                new TestEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Test_2",
                }
            };

            DataMocker<TestEntity> dataMocker = new DataMocker<TestEntity>(initialDataList);

            Assert.Throws<InvalidOperationException>(() => dataMocker.CommitTransaction());
            Assert.Throws<InvalidOperationException>(() => dataMocker.RollbackTransaction());

            dataMocker.BeginTransaction();
            Assert.Throws<InvalidOperationException>(() => dataMocker.BeginTransaction());

            dataMocker.Add(testData);

            Assert.That(dataMocker.AddList.Count, Is.EqualTo(1));
            Assert.That(dataMocker.AddListCount(), Is.EqualTo(1));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(1));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(1));

            dataMocker.SaveChanges();

            Assert.That(dataMocker.AddList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.AddListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(1));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(2));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(2));

            dataMocker.CommitTransaction();

            Assert.That(dataMocker.AddList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.AddListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(2));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(2));

            dataMocker.BeginTransaction();
            dataMocker.AddRange(testDataList);

            Assert.That(dataMocker.AddList.Count, Is.EqualTo(2));
            Assert.That(dataMocker.AddListCount(), Is.EqualTo(2));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(2));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(2));

            dataMocker.SaveChanges();

            Assert.That(dataMocker.AddList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.AddListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(2));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(4));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(4));

            dataMocker.RollbackTransaction();

            Assert.That(dataMocker.AddList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.AddListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(2));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(2));

            dataMocker.Add(testDataSecondary);
            dataMocker.SaveChanges();

            Assert.That(dataMocker.AddList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.AddListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(3));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(3));

            dataMocker.BeginTransaction();
            dataMocker.Remove(testDataSecondary);

            Assert.That(dataMocker.RemoveList.Count, Is.EqualTo(1));
            Assert.That(dataMocker.RemoveListCount(), Is.EqualTo(1));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(3));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(3));

            dataMocker.SaveChanges();

            Assert.That(dataMocker.RemoveList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.RemoveListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(3));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(2));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(2));

            dataMocker.CommitTransaction();

            Assert.That(dataMocker.RemoveList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.RemoveListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(2));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(2));

            dataMocker.AddRange(testDataList);
            dataMocker.SaveChanges();

            Assert.That(dataMocker.AddList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.AddListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(4));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(4));

            dataMocker.BeginTransaction();
            dataMocker.RemoveRange(testDataList);

            Assert.That(dataMocker.RemoveList.Count, Is.EqualTo(2));
            Assert.That(dataMocker.RemoveListCount(), Is.EqualTo(2));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(4));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(4));

            dataMocker.SaveChanges();

            Assert.That(dataMocker.RemoveList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.RemoveListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(4));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(2));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(2));

            dataMocker.RollbackTransaction();

            Assert.That(dataMocker.RemoveList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.RemoveListCount(), Is.EqualTo(0));

            Assert.That(dataMocker.RollbackList.Count, Is.EqualTo(0));

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(4));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(4));
        }

        [Test]
        public void Verify_DataMocker_Transactions_Works_InUsingStatement()
        {
            var testData = new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Test",
            };

            var testDataSecondary = new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Test_Secondary",
            };

            DataMocker<TestEntity> dataMocker = new DataMocker<TestEntity>();

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(0));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(0));

            using (var transaction = dataMocker.BeginTransaction())
            {
                dataMocker.Add(testData);
                dataMocker.SaveChanges();

                transaction.Commit();
            }

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(1));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(1));

            using (var transaction = dataMocker.BeginTransaction())
            {
                dataMocker.Add(testDataSecondary);
                dataMocker.SaveChanges();

                transaction.Rollback();
            }

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(1));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(1));

            using (var transaction = dataMocker.BeginTransaction())
            {
                dataMocker.Remove(testData);
                dataMocker.SaveChanges();
            }

            Assert.That(dataMocker.FinalList.Count, Is.EqualTo(1));
            Assert.That(dataMocker.GetAll().Count, Is.EqualTo(1));
        }
    }
}

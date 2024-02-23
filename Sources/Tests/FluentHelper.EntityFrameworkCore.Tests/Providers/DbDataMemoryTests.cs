using FluentHelper.EntityFrameworkCore.InMemory.DbMemory;
using FluentHelper.EntityFrameworkCore.Tests.Support;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentHelper.EntityFrameworkCore.Tests.Providers
{
    [TestFixture]
    public class DbDataMemoryTests
    {
        [Test]
        public void Verify_DbDataMemory_WorksCorrectly_WithInitialData()
        {
            var initialData = new List<TestEntity>()
            {
                new TestEntity
                {
                    Description = "Description_01",
                    Id = Guid.NewGuid(),
                    Name = "Name_01"
                },
                new TestEntity
                {
                    Description = "Description_02",
                    Id = Guid.NewGuid(),
                    Name = "Name_02"
                }
            };

            var dbDataMemory = new DbDataMemory<TestEntity>(initialData);
            var currentList = dbDataMemory.GetAll().ToList();

            ClassicAssert.IsNotNull(currentList);
            ClassicAssert.AreEqual(2, currentList.Count);
        }

        [Test]
        public void Verify_DbDataMemory_WorksCorrectly_AddingData()
        {
            var dbDataMemory = new DbDataMemory<TestEntity>();

            dbDataMemory.Add(new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Added",
                Description = "Description"
            });
            ClassicAssert.AreEqual(1, dbDataMemory.AddListLength);

            var currentList = dbDataMemory.GetAll().ToList();
            ClassicAssert.AreEqual(0, currentList.Count);

            dbDataMemory.SaveChanges();
            ClassicAssert.AreEqual(0, dbDataMemory.AddListLength);

            var saveList = dbDataMemory.GetAll().ToList();
            ClassicAssert.AreEqual(1, saveList.Count);
        }

        [Test]
        public void Verify_DbDataMemory_WorksCorrectly_AddingRangeData()
        {
            var dataToAdd = new List<TestEntity>()
            {
                new TestEntity
                {
                    Description = "Description_01",
                    Id = Guid.NewGuid(),
                    Name = "Name_01"
                },
                new TestEntity
                {
                    Description = "Description_02",
                    Id = Guid.NewGuid(),
                    Name = "Name_02"
                }
            };

            var dbDataMemory = new DbDataMemory<TestEntity>();

            dbDataMemory.AddRange(dataToAdd);
            ClassicAssert.AreEqual(2, dbDataMemory.AddListLength);

            var currentList = dbDataMemory.GetAll().ToList();
            ClassicAssert.AreEqual(0, currentList.Count);

            dbDataMemory.SaveChanges();
            ClassicAssert.AreEqual(0, dbDataMemory.AddListLength);

            var saveList = dbDataMemory.GetAll().ToList();
            ClassicAssert.AreEqual(2, saveList.Count);
        }

        [Test]
        public void Verify_DbDataMemory_WorksCorrectly_RemovingData()
        {
            var initialData = new List<TestEntity>()
            {
                new TestEntity
                {
                    Description = "Description_01",
                    Id = Guid.NewGuid(),
                    Name = "Name_01"
                },
                new TestEntity
                {
                    Description = "Description_02",
                    Id = Guid.NewGuid(),
                    Name = "Name_02"
                }
            };

            var dbDataMemory = new DbDataMemory<TestEntity>(initialData);

            dbDataMemory.Remove(initialData[0]);
            ClassicAssert.AreEqual(1, dbDataMemory.RemoveListLength);

            var currentList = dbDataMemory.GetAll().ToList();
            ClassicAssert.AreEqual(2, currentList.Count);

            dbDataMemory.SaveChanges();
            ClassicAssert.AreEqual(0, dbDataMemory.RemoveListLength);

            var saveList = dbDataMemory.GetAll().ToList();
            ClassicAssert.AreEqual(1, saveList.Count);
        }

        [Test]
        public void Verify_DbDataMemory_WorksCorrectly_RemovingRangeData()
        {
            var initialData = new List<TestEntity>()
            {
                new TestEntity
                {
                    Description = "Description_01",
                    Id = Guid.NewGuid(),
                    Name = "Name_01"
                },
                new TestEntity
                {
                    Description = "Description_02",
                    Id = Guid.NewGuid(),
                    Name = "Name_02"
                }
            };

            var dbDataMemory = new DbDataMemory<TestEntity>(initialData);

            dbDataMemory.RemoveRange(initialData);
            ClassicAssert.AreEqual(2, dbDataMemory.RemoveListLength);

            var currentList = dbDataMemory.GetAll().ToList();
            ClassicAssert.AreEqual(2, currentList.Count);

            dbDataMemory.SaveChanges();
            ClassicAssert.AreEqual(0, dbDataMemory.RemoveListLength);

            var saveList = dbDataMemory.GetAll().ToList();
            ClassicAssert.AreEqual(0, saveList.Count);
        }

        [Test]
        public void Verify_DbDataMemory_WorksCorrectly_WithTransactions()
        {
            var dbDataMemory = new DbDataMemory<TestEntity>();

            using (var transaction = dbDataMemory.BeginTransaction())
            {
                dbDataMemory.Add(new TestEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Added",
                    Description = "Description"
                });
                ClassicAssert.AreEqual(1, dbDataMemory.AddListLength);

                dbDataMemory.SaveChanges();
                ClassicAssert.AreEqual(0, dbDataMemory.AddListLength);

                var saveList = dbDataMemory.GetAll().ToList();
                ClassicAssert.AreEqual(1, saveList.Count);
            }

            var rolledBackList = dbDataMemory.GetAll().ToList();
            ClassicAssert.AreEqual(0, rolledBackList.Count);

            using (var transaction = dbDataMemory.BeginTransaction())
            {
                dbDataMemory.Add(new TestEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Added",
                    Description = "Description"
                });
                ClassicAssert.AreEqual(1, dbDataMemory.AddListLength);

                dbDataMemory.SaveChanges();
                ClassicAssert.AreEqual(0, dbDataMemory.AddListLength);

                var saveList = dbDataMemory.GetAll().ToList();
                ClassicAssert.AreEqual(1, saveList.Count);

                transaction.Commit();
            }

            var committedList = dbDataMemory.GetAll().ToList();
            ClassicAssert.AreEqual(1, committedList.Count);
        }
    }
}

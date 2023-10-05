using FluentHelper.EntityFrameworkCore.InMemory.DbMemory;
using FluentHelper.EntityFrameworkCore.Tests.Support;
using NUnit.Framework;
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

            Assert.IsNotNull(currentList);
            Assert.AreEqual(2, currentList.Count);
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
            Assert.AreEqual(1, dbDataMemory.AddListCount());

            var currentList = dbDataMemory.GetAll().ToList();
            Assert.AreEqual(0, currentList.Count);

            dbDataMemory.SaveChanges();
            Assert.AreEqual(0, dbDataMemory.AddListCount());

            var saveList = dbDataMemory.GetAll().ToList();
            Assert.AreEqual(1, saveList.Count);
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
            Assert.AreEqual(2, dbDataMemory.AddListCount());

            var currentList = dbDataMemory.GetAll().ToList();
            Assert.AreEqual(0, currentList.Count);

            dbDataMemory.SaveChanges();
            Assert.AreEqual(0, dbDataMemory.AddListCount());

            var saveList = dbDataMemory.GetAll().ToList();
            Assert.AreEqual(2, saveList.Count);
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
            Assert.AreEqual(1, dbDataMemory.RemoveListCount());

            var currentList = dbDataMemory.GetAll().ToList();
            Assert.AreEqual(2, currentList.Count);

            dbDataMemory.SaveChanges();
            Assert.AreEqual(0, dbDataMemory.RemoveListCount());

            var saveList = dbDataMemory.GetAll().ToList();
            Assert.AreEqual(1, saveList.Count);
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
            Assert.AreEqual(2, dbDataMemory.RemoveListCount());

            var currentList = dbDataMemory.GetAll().ToList();
            Assert.AreEqual(2, currentList.Count);

            dbDataMemory.SaveChanges();
            Assert.AreEqual(0, dbDataMemory.RemoveListCount());

            var saveList = dbDataMemory.GetAll().ToList();
            Assert.AreEqual(0, saveList.Count);
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
                Assert.AreEqual(1, dbDataMemory.AddListCount());

                dbDataMemory.SaveChanges();
                Assert.AreEqual(0, dbDataMemory.AddListCount());

                var saveList = dbDataMemory.GetAll().ToList();
                Assert.AreEqual(1, saveList.Count);
            }

            var rolledBackList = dbDataMemory.GetAll().ToList();
            Assert.AreEqual(0, rolledBackList.Count);

            using (var transaction = dbDataMemory.BeginTransaction())
            {
                dbDataMemory.Add(new TestEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Added",
                    Description = "Description"
                });
                Assert.AreEqual(1, dbDataMemory.AddListCount());

                dbDataMemory.SaveChanges();
                Assert.AreEqual(0, dbDataMemory.AddListCount());

                var saveList = dbDataMemory.GetAll().ToList();
                Assert.AreEqual(1, saveList.Count);

                transaction.Commit();
            }

            var committedList = dbDataMemory.GetAll().ToList();
            Assert.AreEqual(1, committedList.Count);
        }
    }
}

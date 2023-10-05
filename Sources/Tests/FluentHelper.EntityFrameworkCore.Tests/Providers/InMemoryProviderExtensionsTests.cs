using FluentHelper.EntityFrameworkCore.InMemory;
using FluentHelper.EntityFrameworkCore.Interfaces;
using FluentHelper.EntityFrameworkCore.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentHelper.EntityFrameworkCore.Tests.Providers
{
    [TestFixture]
    public class InMemoryProviderExtensionsTests
    {
        [TearDown]
        public void TearDown()
        {
            InMemoryProviderExtensions.ClearMemoryContext();
        }

        [Test]
        public void Verify_AddInMemoryContext_WorksCorrectly()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddInMemoryContext();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var dbContext = serviceProvider.GetRequiredService<IDbContext>();

            Assert.IsNotNull(dbContext);
            Assert.AreEqual("FluentInMemory", dbContext.GetProviderName());
        }

        [Test]
        public void Verify_AddMemoryContextSupportTo_WorksCorrectly()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddInMemoryContext();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var dbContext = serviceProvider.GetRequiredService<IDbContext>();

            Assert.IsNotNull(dbContext);

            InMemoryProviderExtensions.AddMemoryContextSupportTo<TestEntity>();

            var testEmptyList = dbContext.Query<TestEntity>().ToList();
            Assert.IsNotNull(testEmptyList);
            Assert.AreEqual(0, testEmptyList.Count);

            dbContext.Add(new TestEntity
            {
                Description = "Description",
                Id = Guid.NewGuid(),
                Name = "Name"
            });
            dbContext.SaveChanges();

            var shouldContainElementList = dbContext.Query<TestEntity>().ToList();
            Assert.IsNotNull(shouldContainElementList);
            Assert.AreEqual(1, shouldContainElementList.Count);
        }

        [Test]
        public void Verify_AddMemoryContextSupportTo_WorksCorrectly_WithInitialData()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddInMemoryContext();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var dbContext = serviceProvider.GetRequiredService<IDbContext>();

            Assert.IsNotNull(dbContext);

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

            InMemoryProviderExtensions.AddMemoryContextSupportTo<TestEntity>(initialData);

            var initializedList = dbContext.Query<TestEntity>().ToList();
            Assert.IsNotNull(initializedList);
            Assert.AreEqual(2, initializedList.Count);
        }

        [Test]
        public void Verify_MemoryContext_DoesNotWork_If_SupportToIsNotCalled()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddInMemoryContext();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var dbContext = serviceProvider.GetRequiredService<IDbContext>();

            Assert.IsNotNull(dbContext);

            var testEmptyList = dbContext.Query<TestEntity>().ToList();
            Assert.IsNotNull(testEmptyList);
            Assert.AreEqual(0, testEmptyList.Count);

            dbContext.Add(new TestEntity
            {
                Description = "Description",
                Id = Guid.NewGuid(),
                Name = "Name"
            });
            dbContext.SaveChanges();

            var shouldNotContainElementList = dbContext.Query<TestEntity>().ToList();
            Assert.IsNotNull(shouldNotContainElementList);
            Assert.AreEqual(0, shouldNotContainElementList.Count);
        }
    }
}

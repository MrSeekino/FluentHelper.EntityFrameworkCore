using Bogus;
using EntityFramework.FluentHelperCore.Examples.Models;
using EntityFramework.FluentHelperCore.Examples.Repositories;
using EntityFramework.FluentHelperCore.Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace EntityFramework.FluentHelperCore.Examples.Tests
{
    [TestFixture(Category = "Repository: TestData")]
    public class TestDataRepositoryTests
    {
        [Test]
        public void Can_Get_List()
        {
            var dataGenerator = new Faker<TestData>().StrictMode(false)
                                .RuleFor(u => u.Id, f => Guid.NewGuid())
                                .RuleFor(u => u.Name, f => f.Name.FirstName())
                                .RuleFor(u => u.Active, f => true)
                                .RuleFor(u => u.CreationDate, f => DateTime.UtcNow);

            var dataSet = dataGenerator.Generate(3);

            var mockContext = new DbContextMocker();
            mockContext.AddSupportTo(dataSet);

            var repository = new TestDataRepository(mockContext.Object);
            var result = repository.GetAll();

            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public void Can_Add_Data()
        {
            var dataGenerator = new Faker<TestData>().StrictMode(false)
                                .RuleFor(u => u.Id, f => Guid.NewGuid())
                                .RuleFor(u => u.Name, f => f.Name.FirstName())
                                .RuleFor(u => u.Active, f => true)
                                .RuleFor(u => u.CreationDate, f => DateTime.UtcNow);

            var dataSet = dataGenerator.Generate(1);

            var mockContext = new DbContextMocker();
            mockContext.AddSupportTo(dataSet);

            var repository = new TestDataRepository(mockContext.Object);
            repository.Add(new TestData
            {
                Id = Guid.NewGuid(),
                Name = "NewData",
                CreationDate = DateTime.UtcNow,
                Active = true
            });

            var result = repository.GetAll();
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void Can_Update_Data()
        {
            var dataGenerator = new Faker<TestData>().StrictMode(false)
                                .RuleFor(u => u.Id, f => Guid.NewGuid())
                                .RuleFor(u => u.Name, f => f.Name.FirstName())
                                .RuleFor(u => u.Active, f => true)
                                .RuleFor(u => u.CreationDate, f => DateTime.UtcNow);

            var dataSet = dataGenerator.Generate(1);

            var mockContext = new DbContextMocker();
            mockContext.AddSupportTo(dataSet);

            var testDataUpdate = new TestData
            {
                Id = dataSet[0].Id,
                Name = "UpdateData",
                Active = true
            };

            var repository = new TestDataRepository(mockContext.Object);
            repository.Update(testDataUpdate);

            var result = repository.GetAll().ToList();
            Assert.AreEqual(1, result.Count());
            Assert.That(result[0].Name == testDataUpdate.Name);
        }

        [Test]
        public void Can_Remove_Data()
        {
            var dataGenerator = new Faker<TestData>().StrictMode(false)
                                .RuleFor(u => u.Id, f => Guid.NewGuid())
                                .RuleFor(u => u.Name, f => f.Name.FirstName())
                                .RuleFor(u => u.Active, f => true)
                                .RuleFor(u => u.CreationDate, f => DateTime.UtcNow);

            var dataSet = dataGenerator.Generate(3);

            var mockContext = new DbContextMocker();
            mockContext.AddSupportTo(dataSet);

            var repository = new TestDataRepository(mockContext.Object);
            repository.Remove(dataSet[1].Id);

            var result = repository.GetAll();
            Assert.AreEqual(2, result.Count());
        }
    }
}

using FluentHelper.EntityFrameworkCore.Examples.Models;
using FluentHelper.EntityFrameworkCore.Examples.Repositories;
using Xunit;

namespace FluentHelper.EntityFrameworkCore.Examples.Tests
{
    public class TestDataRepositoryTests : BaseRepositoriesTests<TestDataRepository>
    {
        public TestDataRepositoryTests()
        {
            var initialDataList = new List<TestData>() { new TestData(), new TestData() };

            AddSupportTo(initialDataList);
        }

        [Fact]
        public void Can_GetAll()
        {
            var retrievedDataList = Repository.GetAll();

            Assert.NotNull(retrievedDataList);
            Assert.Equal(2, retrievedDataList.Count());
        }

        [Fact]
        public void Can_Add_TestData()
        {
            var dataToAdd = new TestData
            {
                Id = Guid.NewGuid(),
                Active = true,
                CreationDate = DateTime.UtcNow,
                Name = "TestData",
                Attr = null,
                ChildList = new List<TestChild>()
            };

            Repository.Add(dataToAdd);
            var retrievedData = Repository.GetById(dataToAdd.Id);

            Assert.NotNull(retrievedData);
            Assert.Equal(dataToAdd, retrievedData);
        }
    }
}
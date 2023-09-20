using FluentHelper.EntityFrameworkCore.Examples.Repositories;

namespace FluentHelper.EntityFrameworkCore.Examples.Tests
{
    public class TestDataRepositoryTests : BaseRepositoriesTests<TestDataRepository>
    {
        //[Fact]
        //public void Can_Add_TestData()
        //{
        //    var dataToAdd = new TestData
        //    {
        //        Id = Guid.NewGuid(),
        //        Active = true,
        //        CreationDate = DateTime.UtcNow,
        //        Name = "TestData",
        //        Attr = null,
        //        ChildList = new List<TestChild>()
        //    };

        //    Repository.Add(dataToAdd);
        //    var retrievedData = Repository.GetById(dataToAdd.Id);

        //    Assert.NotNull(retrievedData);
        //    Assert.Equal(dataToAdd, retrievedData);
        //}
    }
}
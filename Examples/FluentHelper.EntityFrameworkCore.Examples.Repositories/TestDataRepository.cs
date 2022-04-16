using FluentHelper.EntityFrameworkCore.Examples.Models;
using FluentHelper.EntityFrameworkCore.Interfaces;
using System;
using System.Linq;

namespace FluentHelper.EntityFrameworkCore.Examples.Repositories
{
    public class TestDataRepository : BaseRepository
    {
        public TestDataRepository() : base() { }

        public TestDataRepository(IDbContext dbContext) : base(dbContext) { }

        public IQueryable<TestData> GetAll()
        {
            return DbContext.Query<TestData>();
        }

        public TestData GetById(Guid id)
        {
            return DbContext.Query<TestData>().SingleOrDefault(e => e.Id == id);
        }

        public void Add(TestData testData)
        {
            DbContext.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

            DbContext.Add(testData);
            DbContext.SaveChanges();

            DbContext.CommitTransaction();
        }

        public void Update(TestData testData)
        {
            var testDataInstance = DbContext.Query<TestData>().SingleOrDefault(x => x.Id == testData.Id);
            if (testDataInstance != null)
            {
                testDataInstance.Name = testData.Name;
                testDataInstance.Active = testData.Active;

                DbContext.SaveChanges();
            }
        }

        public void Remove(Guid id)
        {
            var testDataInstance = DbContext.Query<TestData>().SingleOrDefault(e => e.Id == id);
            if (testDataInstance != null)
            {
                //if (testDataInstance.ChildList != null && testDataInstance.ChildList.Any())
                //    foreach (var childInstance in testDataInstance.ChildList)
                //        DbContext.Remove(childInstance);

                DbContext.Remove(testDataInstance);
                DbContext.SaveChanges();
            }
        }

        public void AddChild(TestChild testChild)
        {
            DbContext.Add(testChild);
            DbContext.SaveChanges();
        }

        public void RemoveChild(Guid id)
        {
            var testChildInstance = DbContext.Query<TestChild>().SingleOrDefault(e => e.Id == id);
            if (testChildInstance != null)
            {
                DbContext.Remove(testChildInstance);
                DbContext.SaveChanges();
            }
        }

        public void AddAttr(TestDataAttr testAttr)
        {
            DbContext.Add(testAttr);
            DbContext.SaveChanges();
        }

        public void RemoveAttr(Guid id)
        {
            var testAttrInstance = DbContext.Query<TestDataAttr>().SingleOrDefault(e => e.Id == id);
            if (testAttrInstance != null)
            {
                DbContext.Remove(testAttrInstance);
                DbContext.SaveChanges();
            }
        }
    }
}

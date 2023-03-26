using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FluentHelper.EntityFrameworkCore.Tests")]
namespace FluentHelper.EntityFrameworkCore.Moq
{
    public class DbContextMocker
    {
        internal bool HasActiveTransaction { get; set; }

        internal Dictionary<Type, IDataMocker> MockData { get; set; }
        internal Mock<IDbContext> MockContext { get; set; }

        public IDbContext Object
        {
            get
            {
                return MockContext.Object;
            }
        }

        public DbContextMocker()
        {
            MockData = new Dictionary<Type, IDataMocker>();

            MockContext = new Mock<IDbContext>();
            MockContext.Setup(x => x.AreSavepointsSupported()).Returns(false);
            MockContext.Setup(x => x.CreateSavepoint(It.IsAny<string>())).Throws(new NotSupportedException("Savepoints are not supported in DbContextMocker"));
            MockContext.Setup(x => x.ReleaseSavepoint(It.IsAny<string>())).Throws(new NotSupportedException("Savepoints are not supported in DbContextMocker"));
            MockContext.Setup(x => x.RollbackToSavepoint(It.IsAny<string>())).Throws(new NotSupportedException("Savepoints are not supported in DbContextMocker"));

            MockContext.Setup(c => c.SaveChanges()).Callback(() =>
            {
                foreach (var mockData in MockData)
                    mockData.Value.SaveChanges();
            });

            MockContext.Setup(x => x.IsTransactionOpen()).Returns(HasActiveTransaction);
            MockContext.Setup(c => c.BeginTransaction()).Callback(() =>
            {
                if (HasActiveTransaction)
                    throw new Exception("There is already a transaction opened");

                HasActiveTransaction = true;

                foreach (var mockData in MockData)
                    mockData.Value.BeginTransaction();
            });
            MockContext.Setup(c => c.ExecuteOnDatabase(It.IsAny<Func<DatabaseFacade, IDbContextTransaction>>())).Callback(() =>
            {
                if (HasActiveTransaction)
                    throw new Exception("There is already a transaction opened");

                HasActiveTransaction = true;

                foreach (var mockData in MockData)
                    mockData.Value.BeginTransaction();
            });
            MockContext.Setup(c => c.RollbackTransaction()).Callback(() =>
            {
                if (!HasActiveTransaction)
                    throw new Exception("No Open Transaction found");

                HasActiveTransaction = false;

                foreach (var mockData in MockData)
                    mockData.Value.RollbackTransaction();
            });
            MockContext.Setup(c => c.CommitTransaction()).Callback(() =>
            {
                if (!HasActiveTransaction)
                    throw new Exception("No Open Transaction found");

                HasActiveTransaction = false;

                foreach (var mockData in MockData)
                    mockData.Value.CommitTransaction();
            });
        }

        public void AddSupportTo<T>(IEnumerable<T>? initialData = null) where T : class
        {
            MockData.Add(typeof(T), new DataMocker<T>(initialData));

            MockContext.Setup(c => c.Query<T>()).Returns(((IDataMocker<T>)MockData[typeof(T)]).GetAll());

            MockContext.Setup(c => c.Add(It.IsAny<T>())).Callback<T>(x => ((IDataMocker<T>)MockData[typeof(T)]).Add(x));
            MockContext.Setup(c => c.AddRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(x => ((IDataMocker<T>)MockData[typeof(T)]).AddRange(x));

            MockContext.Setup(c => c.Remove(It.IsAny<T>())).Callback<T>(x => ((IDataMocker<T>)MockData[typeof(T)]).Remove(x));
            MockContext.Setup(c => c.RemoveRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(x => ((IDataMocker<T>)MockData[typeof(T)]).RemoveRange(x));
        }

        public void AddMockToExecuteOnDatabase<T>(DatabaseFacade databaseFacade, Action<T> callBackFunc)
        {
            if (typeof(T) == typeof(IDbContextTransaction))
                throw new ArgumentException($"{typeof(IDbContextTransaction).Name} cannot be mocked with ExecuteOnDatabase");

            MockContext.Setup(c => c.ExecuteOnDatabase(It.IsAny<Func<DatabaseFacade, T>>())).Callback<Func<DatabaseFacade, T>>(funcResult =>
            {
                callBackFunc(funcResult(databaseFacade));
            });
        }
    }
}

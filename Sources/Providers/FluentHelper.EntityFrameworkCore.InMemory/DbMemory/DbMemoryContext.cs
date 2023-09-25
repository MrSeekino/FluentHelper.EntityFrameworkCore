using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
using NSubstitute.ClearExtensions;
using System.Linq.Expressions;

namespace FluentHelper.EntityFrameworkCore.InMemory.DbMemory
{
    internal class DbMemoryContext : IDisposable
    {
        static DbMemoryContext? Instance { get; set; }

        internal bool HasActiveTransaction { get; set; }

        internal Dictionary<Type, IDbDataMemory> DataMemoryDict { get; set; }
        internal IDbContext FakeContext { get; set; }

        public IDbContext DbContext => FakeContext;

        public static DbMemoryContext GetOrCreate()
        {
            if (Instance == null)
                Instance = new DbMemoryContext();

            return Instance;
        }

        private DbMemoryContext()
        {
            HasActiveTransaction = false;

            DataMemoryDict = new Dictionary<Type, IDbDataMemory>();

            FakeContext = Substitute.For<IDbContext>();

            FakeContext.When(x => x.ExecuteOnDatabase(Arg.Any<Action<DatabaseFacade>>())).Throw(x => new NotSupportedException("ExecuteOnDatabase is not supported"));
            FakeContext.When(x => x.ExecuteSqlRaw(Arg.Any<string>(), Arg.Any<object[]>())).Throw(x => new NotSupportedException("ExecuteSqlRaw is not supported"));
            FakeContext.When(x => x.ExecuteSqlRawAsync(Arg.Any<string>(), Arg.Any<IEnumerable<object>>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("ExecuteSqlRawAsync is not supported"));
            FakeContext.When(x => x.ExecuteSqlRawAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("ExecuteSqlRawAsync is not supported"));

            FakeContext.CanConnect().Returns(true);
            FakeContext.CanConnectAsync(Arg.Any<CancellationToken>()).Returns(true);

            FakeContext.AreSavepointsSupported().Returns(false);
            FakeContext.When(x => x.CreateSavepoint(Arg.Any<string>())).Throw(x => new NotSupportedException("Savepoints are not supported"));
            FakeContext.When(x => x.CreateSavepointAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("Savepoints are not supported"));
            FakeContext.When(x => x.ReleaseSavepoint(Arg.Any<string>())).Throw(x => new NotSupportedException("Savepoints are not supported in DbContextMocker"));
            FakeContext.When(x => x.ReleaseSavepointAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("Savepoints are not supported"));
            FakeContext.When(x => x.RollbackToSavepoint(Arg.Any<string>())).Throw(x => new NotSupportedException("Savepoints are not supported"));
            FakeContext.When(x => x.RollbackToSavepointAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("Savepoints are not supported"));

            FakeContext.When(x => x.SaveChanges()).Do(x =>
            {
                foreach (var dataMemory in DataMemoryDict)
                    dataMemory.Value.SaveChanges();
            });
            FakeContext.When(x => x.SaveChangesAsync(Arg.Any<CancellationToken>())).Do(x =>
            {
                foreach (var dataMemory in DataMemoryDict)
                    dataMemory.Value.SaveChanges();
            });

            FakeContext.IsTransactionOpen().Returns(HasActiveTransaction);
            FakeContext.When(x => x.BeginTransaction()).Do(x =>
            {
                if (HasActiveTransaction)
                    throw new InvalidOperationException("There is already a transaction opened");

                HasActiveTransaction = true;

                foreach (var dataMemory in DataMemoryDict)
                    dataMemory.Value.BeginTransaction();
            });
            FakeContext.When(x => x.BeginTransactionAsync(Arg.Any<CancellationToken>())).Do(x =>
            {
                if (HasActiveTransaction)
                    throw new InvalidOperationException("There is already a transaction opened");

                HasActiveTransaction = true;

                foreach (var dataMemory in DataMemoryDict)
                    dataMemory.Value.BeginTransaction();
            });

            FakeContext.When(x => x.ExecuteOnDatabase(Arg.Any<Func<DatabaseFacade, IDbContextTransaction>>())).Do(x =>
            {
                if (HasActiveTransaction)
                    throw new InvalidOperationException("There is already a transaction opened");

                HasActiveTransaction = true;

                foreach (var dataMemory in DataMemoryDict)
                    dataMemory.Value.BeginTransaction();
            });

            FakeContext.When(x => x.RollbackTransaction()).Do(x =>
            {
                if (!HasActiveTransaction)
                    throw new InvalidOperationException("No Open Transaction found");

                HasActiveTransaction = false;

                foreach (var dataMemory in DataMemoryDict)
                    dataMemory.Value.RollbackTransaction();
            });
            FakeContext.When(x => x.RollbackTransactionAsync(Arg.Any<CancellationToken>())).Do(x =>
            {
                if (!HasActiveTransaction)
                    throw new InvalidOperationException("No Open Transaction found");

                HasActiveTransaction = false;

                foreach (var dataMemory in DataMemoryDict)
                    dataMemory.Value.RollbackTransaction();
            });

            FakeContext.When(x => x.CommitTransaction()).Do(x =>
            {
                if (!HasActiveTransaction)
                    throw new InvalidOperationException("No Open Transaction found");

                HasActiveTransaction = false;

                foreach (var dataMemory in DataMemoryDict)
                    dataMemory.Value.CommitTransaction();
            });
            FakeContext.When(x => x.CommitTransactionAsync(Arg.Any<CancellationToken>())).Do(x =>
            {
                if (!HasActiveTransaction)
                    throw new InvalidOperationException("No Open Transaction found");

                HasActiveTransaction = false;

                foreach (var dataMemory in DataMemoryDict)
                    dataMemory.Value.CommitTransaction();
            });
        }

        public void AddSupportTo<T>(IEnumerable<T>? initialData = null) where T : class
        {
            DataMemoryDict.Add(typeof(T), new DbDataMemory<T>(initialData));

            FakeContext.When(x => x.QueryRaw<T>(Arg.Any<string>(), Arg.Any<object[]>())).Throw(x => new NotSupportedException("QueryRaw is not supported"));
            FakeContext.When(x => x.ExecuteOnDatabase(Arg.Any<Func<DatabaseFacade, T>>())).Throw(x => new NotSupportedException("ExecuteOnDatabase is not supported"));
            FakeContext.When(x => x.ExecuteDelete(Arg.Any<Expression<Func<T, bool>>>())).Throw(x => new NotSupportedException("ExecuteDelete is not supported"));
            FakeContext.When(x => x.ExecuteDeleteAsync(Arg.Any<Expression<Func<T, bool>>>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("ExecuteDeleteAsync is not supported"));
            FakeContext.When(x => x.ExecuteUpdate(Arg.Any<Expression<Func<T, bool>>>(), Arg.Any<Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>>())).Throw(x => new NotSupportedException("ExecuteUpdate is not supported"));
            FakeContext.When(x => x.ExecuteUpdateAsync(Arg.Any<Expression<Func<T, bool>>>(), Arg.Any<Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("ExecuteUpdateAsync is not supported"));

            FakeContext.Query<T>().Returns(((IDbDataMemory<T>)DataMemoryDict[typeof(T)]).GetAll());
            FakeContext.QueryNoTracking<T>().Returns(((IDbDataMemory<T>)DataMemoryDict[typeof(T)]).GetAll());

            FakeContext.When(x => x.Add(Arg.Any<T>())).Do(x =>
            {
                T argData = x.Arg<T>();
                ((IDbDataMemory<T>)DataMemoryDict[typeof(T)]).Add(argData);
            });
            FakeContext.When(x => x.AddAsync(Arg.Any<T>(), Arg.Any<CancellationToken>())).Do(x =>
            {
                T argData = x.Arg<T>();
                ((IDbDataMemory<T>)DataMemoryDict[typeof(T)]).Add(argData);
            });

            FakeContext.When(x => x.AddRange(Arg.Any<IEnumerable<T>>())).Do(x =>
            {
                IEnumerable<T> argData = x.Arg<IEnumerable<T>>();
                ((IDbDataMemory<T>)DataMemoryDict[typeof(T)]).AddRange(argData);
            });
            FakeContext.When(x => x.AddRangeAsync(Arg.Any<IEnumerable<T>>(), Arg.Any<CancellationToken>())).Do(x =>
            {
                IEnumerable<T> argData = x.Arg<IEnumerable<T>>();
                ((IDbDataMemory<T>)DataMemoryDict[typeof(T)]).AddRange(argData);
            });

            FakeContext.When(x => x.Remove(Arg.Any<T>())).Do(x =>
            {
                T argData = x.Arg<T>();
                ((IDbDataMemory<T>)DataMemoryDict[typeof(T)]).Remove(argData);
            });
            FakeContext.When(x => x.RemoveRange(Arg.Any<IEnumerable<T>>())).Do(x =>
            {
                IEnumerable<T> argData = x.Arg<IEnumerable<T>>();
                ((IDbDataMemory<T>)DataMemoryDict[typeof(T)]).RemoveRange(argData);
            });
        }

        public void AddSupportToExecuteOnDatabase<T>(DatabaseFacade databaseFacade, Action<T> callBackFunc)
        {
            if (typeof(T) == typeof(IDbContextTransaction))
                throw new ArgumentException($"{typeof(IDbContextTransaction).Name} cannot be mocked with ExecuteOnDatabase");

            FakeContext.When(x => x.ExecuteOnDatabase(Arg.Any<Func<DatabaseFacade, T>>())).Do(x =>
            {
                Func<DatabaseFacade, T> funcResult = x.Arg<Func<DatabaseFacade, T>>();
                callBackFunc(funcResult(databaseFacade));
            });
        }

        public void Dispose()
        {
            DataMemoryDict.Clear();
            FakeContext.ClearSubstitute();
            Instance = null;
        }
    }
}

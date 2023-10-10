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
        private static DbMemoryContext? Instance { get; set; }

        private bool _hasActiveTransaction;
        private Dictionary<Type, IDbDataMemory> _dataMemoryDict;
        private IDbContext _fakeContext;

        public IDbContext DbContext => _fakeContext;

        public static DbMemoryContext GetOrCreate()
        {
            if (Instance == null)
                Instance = new DbMemoryContext();

            return Instance;
        }

        private DbMemoryContext()
        {
            _hasActiveTransaction = false;

            _dataMemoryDict = new Dictionary<Type, IDbDataMemory>();

            _fakeContext = Substitute.For<IDbContext>();
            _fakeContext.GetProviderName().Returns("FluentInMemory");

            _fakeContext.When(x => x.ExecuteOnDatabase(Arg.Any<Action<DatabaseFacade>>())).Throw(x => new NotSupportedException("ExecuteOnDatabase is not supported"));
            _fakeContext.When(x => x.ExecuteSqlRaw(Arg.Any<string>(), Arg.Any<object[]>())).Throw(x => new NotSupportedException("ExecuteSqlRaw is not supported"));
            _fakeContext.When(x => x.ExecuteSqlRawAsync(Arg.Any<string>(), Arg.Any<IEnumerable<object>>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("ExecuteSqlRawAsync is not supported"));
            _fakeContext.When(x => x.ExecuteSqlRawAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("ExecuteSqlRawAsync is not supported"));

            _fakeContext.CanConnect().Returns(true);
            _fakeContext.CanConnectAsync(Arg.Any<CancellationToken>()).Returns(true);

            _fakeContext.AreSavepointsSupported().Returns(false);
            _fakeContext.When(x => x.CreateSavepoint(Arg.Any<string>())).Throw(x => new NotSupportedException("Savepoints are not supported"));
            _fakeContext.When(x => x.CreateSavepointAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("Savepoints are not supported"));
            _fakeContext.When(x => x.ReleaseSavepoint(Arg.Any<string>())).Throw(x => new NotSupportedException("Savepoints are not supported in DbContextMocker"));
            _fakeContext.When(x => x.ReleaseSavepointAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("Savepoints are not supported"));
            _fakeContext.When(x => x.RollbackToSavepoint(Arg.Any<string>())).Throw(x => new NotSupportedException("Savepoints are not supported"));
            _fakeContext.When(x => x.RollbackToSavepointAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("Savepoints are not supported"));

            _fakeContext.When(x => x.SaveChanges()).Do(x =>
            {
                foreach (var dataMemory in _dataMemoryDict)
                    dataMemory.Value.SaveChanges();
            });
            _fakeContext.When(x => x.SaveChangesAsync(Arg.Any<CancellationToken>())).Do(x =>
            {
                foreach (var dataMemory in _dataMemoryDict)
                    dataMemory.Value.SaveChanges();
            });

            _fakeContext.IsTransactionOpen().Returns(_hasActiveTransaction);
            _fakeContext.When(x => x.BeginTransaction()).Do(x =>
            {
                if (_hasActiveTransaction)
                    throw new InvalidOperationException("There is already a transaction opened");

                _hasActiveTransaction = true;

                foreach (var dataMemory in _dataMemoryDict)
                    dataMemory.Value.BeginTransaction();
            });
            _fakeContext.When(x => x.BeginTransactionAsync(Arg.Any<CancellationToken>())).Do(x =>
            {
                if (_hasActiveTransaction)
                    throw new InvalidOperationException("There is already a transaction opened");

                _hasActiveTransaction = true;

                foreach (var dataMemory in _dataMemoryDict)
                    dataMemory.Value.BeginTransaction();
            });

            _fakeContext.When(x => x.ExecuteOnDatabase(Arg.Any<Func<DatabaseFacade, IDbContextTransaction>>())).Do(x =>
            {
                if (_hasActiveTransaction)
                    throw new InvalidOperationException("There is already a transaction opened");

                _hasActiveTransaction = true;

                foreach (var dataMemory in _dataMemoryDict)
                    dataMemory.Value.BeginTransaction();
            });

            _fakeContext.When(x => x.RollbackTransaction()).Do(x =>
            {
                if (!_hasActiveTransaction)
                    throw new InvalidOperationException("No Open Transaction found");

                _hasActiveTransaction = false;

                foreach (var dataMemory in _dataMemoryDict)
                    dataMemory.Value.RollbackTransaction();
            });
            _fakeContext.When(x => x.RollbackTransactionAsync(Arg.Any<CancellationToken>())).Do(x =>
            {
                if (!_hasActiveTransaction)
                    throw new InvalidOperationException("No Open Transaction found");

                _hasActiveTransaction = false;

                foreach (var dataMemory in _dataMemoryDict)
                    dataMemory.Value.RollbackTransaction();
            });

            _fakeContext.When(x => x.CommitTransaction()).Do(x =>
            {
                if (!_hasActiveTransaction)
                    throw new InvalidOperationException("No Open Transaction found");

                _hasActiveTransaction = false;

                foreach (var dataMemory in _dataMemoryDict)
                    dataMemory.Value.CommitTransaction();
            });
            _fakeContext.When(x => x.CommitTransactionAsync(Arg.Any<CancellationToken>())).Do(x =>
            {
                if (!_hasActiveTransaction)
                    throw new InvalidOperationException("No Open Transaction found");

                _hasActiveTransaction = false;

                foreach (var dataMemory in _dataMemoryDict)
                    dataMemory.Value.CommitTransaction();
            });
        }

        public void AddSupportTo<T>(IEnumerable<T>? initialData = null) where T : class
        {
            _dataMemoryDict.Add(typeof(T), new DbDataMemory<T>(initialData));

            _fakeContext.When(x => x.QueryRaw<T>(Arg.Any<string>(), Arg.Any<object[]>())).Throw(x => new NotSupportedException("QueryRaw is not supported"));
            _fakeContext.When(x => x.ExecuteOnDatabase(Arg.Any<Func<DatabaseFacade, T>>())).Throw(x => new NotSupportedException("ExecuteOnDatabase is not supported"));
            _fakeContext.When(x => x.ExecuteDelete(Arg.Any<Expression<Func<T, bool>>>())).Throw(x => new NotSupportedException("ExecuteDelete is not supported"));
            _fakeContext.When(x => x.ExecuteDeleteAsync(Arg.Any<Expression<Func<T, bool>>>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("ExecuteDeleteAsync is not supported"));
            _fakeContext.When(x => x.ExecuteUpdate(Arg.Any<Expression<Func<T, bool>>>(), Arg.Any<Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>>())).Throw(x => new NotSupportedException("ExecuteUpdate is not supported"));
            _fakeContext.When(x => x.ExecuteUpdateAsync(Arg.Any<Expression<Func<T, bool>>>(), Arg.Any<Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>>(), Arg.Any<CancellationToken>())).Throw(x => new NotSupportedException("ExecuteUpdateAsync is not supported"));

            _fakeContext.Query<T>().Returns(((IDbDataMemory<T>)_dataMemoryDict[typeof(T)]).GetAll());
            _fakeContext.QueryNoTracking<T>().Returns(((IDbDataMemory<T>)_dataMemoryDict[typeof(T)]).GetAll());

            _fakeContext.When(x => x.Add(Arg.Any<T>())).Do(x =>
            {
                T argData = x.Arg<T>();
                ((IDbDataMemory<T>)_dataMemoryDict[typeof(T)]).Add(argData);
            });
            _fakeContext.When(x => x.AddAsync(Arg.Any<T>(), Arg.Any<CancellationToken>())).Do(x =>
            {
                T argData = x.Arg<T>();
                ((IDbDataMemory<T>)_dataMemoryDict[typeof(T)]).Add(argData);
            });

            _fakeContext.When(x => x.AddRange(Arg.Any<IEnumerable<T>>())).Do(x =>
            {
                IEnumerable<T> argData = x.Arg<IEnumerable<T>>();
                ((IDbDataMemory<T>)_dataMemoryDict[typeof(T)]).AddRange(argData);
            });
            _fakeContext.When(x => x.AddRangeAsync(Arg.Any<IEnumerable<T>>(), Arg.Any<CancellationToken>())).Do(x =>
            {
                IEnumerable<T> argData = x.Arg<IEnumerable<T>>();
                ((IDbDataMemory<T>)_dataMemoryDict[typeof(T)]).AddRange(argData);
            });

            _fakeContext.When(x => x.Remove(Arg.Any<T>())).Do(x =>
            {
                T argData = x.Arg<T>();
                ((IDbDataMemory<T>)_dataMemoryDict[typeof(T)]).Remove(argData);
            });
            _fakeContext.When(x => x.RemoveRange(Arg.Any<IEnumerable<T>>())).Do(x =>
            {
                IEnumerable<T> argData = x.Arg<IEnumerable<T>>();
                ((IDbDataMemory<T>)_dataMemoryDict[typeof(T)]).RemoveRange(argData);
            });
        }

        public void AddSupportToExecuteOnDatabase<T>(DatabaseFacade databaseFacade, Action<T> callBackFunc)
        {
            if (typeof(T) == typeof(IDbContextTransaction))
                throw new ArgumentException($"{typeof(IDbContextTransaction).Name} cannot be mocked with ExecuteOnDatabase");

            _fakeContext.When(x => x.ExecuteOnDatabase(Arg.Any<Func<DatabaseFacade, T>>())).Do(x =>
            {
                Func<DatabaseFacade, T> funcResult = x.Arg<Func<DatabaseFacade, T>>();
                callBackFunc(funcResult(databaseFacade));
            });
        }

        public void Dispose()
        {
            _dataMemoryDict.Clear();
            _fakeContext.ClearSubstitute();

            Instance = null;
        }
    }
}

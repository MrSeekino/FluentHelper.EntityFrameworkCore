using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FluentHelper.EntityFrameworkCore.Tests")]
namespace FluentHelper.EntityFrameworkCore.InMemory.DbMemory
{
    internal sealed class DbDataMemory<T> : IDbDataMemory<T> where T : class
    {
        private List<T> _rollbackList;
        private List<T> _finalList;

        private List<T> _addList;
        private List<T> _removeList;

        private bool _hasActiveTransaction;
        private IDbContextTransaction _dbContectTransactionMock;

        public int AddListLength => _addList.Count;
        public int RemoveListLength => _removeList.Count;

        public DbDataMemory(IEnumerable<T>? initialData = null)
        {
            _finalList = initialData == null ? new List<T>() : initialData.ToList();

            _rollbackList = new List<T>();
            _addList = new List<T>();
            _removeList = new List<T>();

            _hasActiveTransaction = false;

            _dbContectTransactionMock = Substitute.For<IDbContextTransaction>();

            _dbContectTransactionMock.When(x => x.Commit()).Do(x => CommitTransaction());
            _dbContectTransactionMock.When(x => x.CommitAsync(Arg.Any<CancellationToken>())).Do(x => CommitTransaction());

            _dbContectTransactionMock.When(x => x.Rollback()).Do(x => RollbackTransaction());
            _dbContectTransactionMock.When(x => x.RollbackAsync(Arg.Any<CancellationToken>())).Do(x => RollbackTransaction());

            _dbContectTransactionMock.When(x => x.Dispose()).Do(x => RollbackTransaction(true));
        }

        public IQueryable<T> GetAll()
        {
            return _finalList.AsQueryable();
        }

        public void Add(T input)
        {
            _addList.Add(input);
        }

        public void AddRange(IEnumerable<T> inputList)
        {
            _addList.AddRange(inputList);
        }

        public void Remove(T input)
        {
            _removeList.Add(input);
        }

        public void RemoveRange(IEnumerable<T> inputList)
        {
            _removeList.AddRange(inputList);
        }

        public int SaveChanges()
        {
            _rollbackList.Clear();
            if (_hasActiveTransaction)
                foreach (var item in _finalList)
                    _rollbackList.Add(item);

            int result = _addList.Count + _removeList.Count;

            foreach (var addItem in _addList)
            {
                if (_finalList.Any(x => x.Equals(addItem)))
                    throw new InvalidOperationException("Cannot add twice the same item");

                _finalList.Add(addItem);
            }

            foreach (var removeItem in _removeList)
            {
                if (!_finalList.Any(x => x.Equals(removeItem)))
                    throw new InvalidOperationException("Cannot remove items that are not in the list");

                _finalList.Remove(removeItem);
            }

            _addList.Clear();
            _removeList.Clear();

            return result;
        }

        public IDbContextTransaction BeginTransaction()
        {
            if (_hasActiveTransaction)
                throw new InvalidOperationException("There is already a transaction opened");

            _hasActiveTransaction = true;
            return _dbContectTransactionMock;
        }

        public void CommitTransaction()
        {
            if (!_hasActiveTransaction)
                throw new InvalidOperationException("No Open Transaction found");

            _hasActiveTransaction = false;

            _addList.Clear();
            _removeList.Clear();
            _rollbackList.Clear();
        }

        public void RollbackTransaction(bool noThrow = false)
        {
            if (!_hasActiveTransaction)
            {
                if (noThrow)
                    return;

                throw new InvalidOperationException("No Open Transaction found");
            }

            _hasActiveTransaction = false;

            _finalList.Clear();
            foreach (var item in _rollbackList)
                _finalList.Add(item);

            _addList.Clear();
            _removeList.Clear();
            _rollbackList.Clear();
        }
    }
}

using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FluentHelper.EntityFrameworkCore.Tests")]
namespace FluentHelper.EntityFrameworkCore.InMemory.DbMemory
{
    internal sealed class DbDataMemory<T> : IDbDataMemory<T> where T : class
    {
        internal List<T> RollbackList { get; set; }
        internal List<T> FinalList { get; set; }

        internal List<T> AddList { get; set; }
        internal List<T> RemoveList { get; set; }

        internal bool HasActiveTransaction { get; set; }
        IDbContextTransaction DbContectTransactionMock { get; set; }

        public DbDataMemory(IEnumerable<T>? initialData = null)
        {
            FinalList = initialData == null ? new List<T>() : initialData.ToList();

            RollbackList = new List<T>();
            AddList = new List<T>();
            RemoveList = new List<T>();

            HasActiveTransaction = false;

            DbContectTransactionMock = Substitute.For<IDbContextTransaction>();

            DbContectTransactionMock.When(x => x.Commit()).Do(x => CommitTransaction());
            DbContectTransactionMock.When(x => x.CommitAsync(Arg.Any<CancellationToken>())).Do(x => CommitTransaction());

            DbContectTransactionMock.When(x => x.Rollback()).Do(x => RollbackTransaction());
            DbContectTransactionMock.When(x => x.RollbackAsync(Arg.Any<CancellationToken>())).Do(x => RollbackTransaction());

            DbContectTransactionMock.When(x => x.Dispose()).Do(x => RollbackTransaction(true));
        }

        public int AddListCount()
        {
            return AddList.Count;
        }

        public int RemoveListCount()
        {
            return RemoveList.Count;
        }

        public IQueryable<T> GetAll()
        {
            return FinalList.AsQueryable();
        }

        public void Add(T input)
        {
            AddList.Add(input);
        }

        public void AddRange(IEnumerable<T> inputList)
        {
            AddList.AddRange(inputList);
        }

        public void Remove(T input)
        {
            RemoveList.Add(input);
        }

        public void RemoveRange(IEnumerable<T> inputList)
        {
            RemoveList.AddRange(inputList);
        }

        public int SaveChanges()
        {
            RollbackList.Clear();
            if (HasActiveTransaction)
                foreach (var item in FinalList)
                    RollbackList.Add(item);

            int result = AddList.Count + RemoveList.Count;

            foreach (var addItem in AddList)
            {
                if (FinalList.Any(x => x.Equals(addItem)))
                    throw new InvalidOperationException("Cannot add twice the same item");

                FinalList.Add(addItem);
            }

            foreach (var removeItem in RemoveList)
            {
                if (!FinalList.Any(x => x.Equals(removeItem)))
                    throw new InvalidOperationException("Cannot remove items that are not in the list");

                FinalList.Remove(removeItem);
            }

            AddList.Clear();
            RemoveList.Clear();

            return result;
        }

        public IDbContextTransaction BeginTransaction()
        {
            if (HasActiveTransaction)
                throw new InvalidOperationException("There is already a transaction opened");

            HasActiveTransaction = true;
            return DbContectTransactionMock;
        }

        public void CommitTransaction()
        {
            if (!HasActiveTransaction)
                throw new InvalidOperationException("No Open Transaction found");

            HasActiveTransaction = false;

            AddList.Clear();
            RemoveList.Clear();
            RollbackList.Clear();
        }

        public void RollbackTransaction(bool noThrow = false)
        {
            if (!HasActiveTransaction)
            {
                if (noThrow)
                    return;

                throw new InvalidOperationException("No Open Transaction found");
            }

            HasActiveTransaction = false;

            FinalList.Clear();
            foreach (var item in RollbackList)
                FinalList.Add(item);

            AddList.Clear();
            RemoveList.Clear();
            RollbackList.Clear();
        }
    }
}

using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FluentHelper.EntityFrameworkCore.Tests")]
namespace FluentHelper.EntityFrameworkCore.Moq
{
    internal class DataMocker<T> : IDataMocker<T> where T : class
    {
        internal List<T> RollbackList { get; set; }
        internal List<T> FinalList { get; set; }

        internal List<T> AddList { get; set; }
        internal List<T> RemoveList { get; set; }

        internal bool HasActiveTransaction { get; set; }
        internal Mock<IDbContextTransaction> DbContectTransactionMock { get; set; }

        public DataMocker(IEnumerable<T>? initialData = null)
        {
            FinalList = initialData == null ? new List<T>() : initialData.ToList();

            RollbackList = new List<T>();
            AddList = new List<T>();
            RemoveList = new List<T>();

            HasActiveTransaction = false;

            DbContectTransactionMock = new Mock<IDbContextTransaction>();
            DbContectTransactionMock.Setup(x => x.Commit()).Callback(() => CommitTransaction());
            DbContectTransactionMock.Setup(x => x.Rollback()).Callback(() => RollbackTransaction());
            DbContectTransactionMock.Setup(x => x.Dispose()).Callback(() => RollbackTransaction(true));
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
            return DbContectTransactionMock.Object;
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

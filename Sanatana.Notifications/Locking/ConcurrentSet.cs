using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.Notifications.DAL.Entities;
using System.Threading;

namespace Sanatana.Notifications.Locking
{
    public class ConcurrentSet<TKey>
        where TKey : struct
    {
        //fields
        private readonly ReaderWriterLockSlim _itemsLock;
        protected HashSet<ConsolidationLock<TKey>> _items;


        //ctor
        public ConcurrentSet(IEnumerable<ConsolidationLock<TKey>> items)
        {
            _itemsLock = new ReaderWriterLockSlim();
            _items = new HashSet<ConsolidationLock<TKey>>(items);
        }


        //methods
        public bool AddIfUnique(ConsolidationLock<TKey> item)
        {
            _itemsLock.EnterWriteLock();
            bool added = false;

            try
            {
                ConsolidationLock<TKey> sameGroupLock = _items.FirstOrDefault(x => x == item);
                if (sameGroupLock == null)
                {
                    added = true;
                    _items.Add(item);
                }
            }
            finally
            {
                _itemsLock.ExitWriteLock();
            }

            return added;
        }

        public virtual ConsolidationLock<TKey> GetMatchingGroup(ConsolidationLock<TKey> item)
        {
            _itemsLock.EnterReadLock();
            try
            {
                return _items.FirstOrDefault(x => x == item);
            }
            finally
            {
                _itemsLock.ExitReadLock();
            }
        }

        public virtual ConsolidationLock<TKey>[] GetAll()
        {
            _itemsLock.EnterReadLock();
            try
            {
                return _items.ToArray();
            }
            finally
            {
                _itemsLock.ExitReadLock();
            }
        }

        public void ReplaceGroup(ConsolidationLock<TKey> item)
        {
            if (item == null)
            {
                return;
            }

            _itemsLock.EnterWriteLock();
            try
            {
                ConsolidationLock<TKey> sameGroupLock = _items.FirstOrDefault(x => x == item);
                if (sameGroupLock != null)
                {
                    _items.Remove(sameGroupLock);
                }

                _items.Add(item);
            }
            finally
            {
                _itemsLock.ExitWriteLock();
            }
        }

        public void Remove(IEnumerable<ConsolidationLock<TKey>> items)
        {
            _itemsLock.EnterWriteLock();
            try
            {
                int removedItems = _items.RemoveWhere(x => items.Contains(x));
            }
            finally
            {
                _itemsLock.ExitWriteLock();
            }
        }
    }
}

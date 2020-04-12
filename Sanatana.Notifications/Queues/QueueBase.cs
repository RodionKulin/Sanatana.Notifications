using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Flushing;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Parameters;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Sanatana.Notifications.Models;


namespace Sanatana.Notifications.Queues
{
    public abstract class QueueBase<TSignal, TKey>
        where TKey : struct
    {
        //fields
        protected object _queueLock;
        protected Dictionary<int, Queue<SignalWrapper<TSignal>>> _itemsQueue;
        protected TemporaryStorageParameters _temporaryStorageParameters;
        protected ITemporaryStorage<TSignal> _temporaryStorage;


        //properties
        /// <summary>
        /// Items number limit when exceeded starts flushing queue items to permanent storage.
        /// </summary>
        public int PersistBeginOnItemsCount { get; set; }
        /// <summary>
        /// Target number of queue items when when flishing to permanent storage stops. 
        /// </summary>
        public int PersistEndOnItemsCount { get; set; }
        /// <summary>
        /// Enable storing items in temporary storage while they are processed to prevent data loss in case of power down.
        /// </summary>
        public bool IsTemporaryStorageEnabled { get; set; }



        //init
        public QueueBase(ITemporaryStorage<TSignal> temporaryStorage)
        {
            _temporaryStorage = temporaryStorage;

            _itemsQueue = new Dictionary<int, Queue<SignalWrapper<TSignal>>>();
            _queueLock = new object();
           
        }


        //Append
        public abstract void Append(SignalWrapper<TSignal> item);
        protected virtual void Append(SignalWrapper<TSignal> item, int key)
        {
            AppendToTemporaryStorage(item);
            
            lock (_queueLock)
            {
                if (!_itemsQueue.ContainsKey(key))
                {
                    _itemsQueue.Add(key, new Queue<SignalWrapper<TSignal>>());
                }

                _itemsQueue[key].Enqueue(item);
            }
        }
        protected virtual void AppendToTemporaryStorage(SignalWrapper<TSignal> item)
        {
            if (IsTemporaryStorageEnabled 
                && item.IsPermanentlyStored == false
                && item.TempStorageId == null)
            {
                item.TempStorageId = Guid.NewGuid();
                _temporaryStorage.Insert(_temporaryStorageParameters, item.TempStorageId.Value, item.Signal);
            }            
        }
        public virtual void RestoreFromTemporaryStorage()
        {
            if(!IsTemporaryStorageEnabled)
            {
                return;
            }

            Dictionary<Guid, TSignal> items = _temporaryStorage.Select(_temporaryStorageParameters);
            foreach (KeyValuePair<Guid, TSignal> item in items)
            {
                var signal = new SignalWrapper<TSignal>(item.Value, false)
                {
                    TempStorageId = item.Key
                };
                Append(signal);
            }
        }


        //Count
        public virtual int CountQueueItems()
        {
            lock (_queueLock)
            {
                return _itemsQueue.Sum(p => p.Value.Count);
            }
        }
        public virtual bool CheckIsEmpty(List<int> activeKeys)
        {
            lock (_queueLock)
            {
                return !_itemsQueue.Any(p => activeKeys.Contains(p.Key) && p.Value.Count > 0);
            }
        }


        //Apply results
        public abstract void ApplyResult(SignalWrapper<TSignal> item, ProcessingResult result);
     

        //Flush results
        public virtual void Flush()
        {
            lock (_queueLock)
            {
                foreach (KeyValuePair<int, Queue<SignalWrapper<TSignal>>> queue in _itemsQueue)
                {
                    while (queue.Value.Count > 0)
                    {
                        SignalWrapper<TSignal> next = queue.Value.Dequeue();
                        ApplyResult(next, ProcessingResult.ReturnToStorage);
                    }
                }
            }
        }


        //Return to persistent storage
        public virtual void ReturnExtraItems(List<int> activeKeys = null)
        {
            lock (_queueLock)
            {
                int currentItemsCount = _itemsQueue.Sum(p => p.Value.Count);
                int targetItemsCount = PersistEndOnItemsCount;
                int extraItems = currentItemsCount - targetItemsCount;

                if (extraItems > 0)
                {
                    if(activeKeys == null)
                    {
                        activeKeys = _itemsQueue.Select(p => p.Key).ToList();
                    }

                    List<int> notActiveKeys = _itemsQueue
                        .Select(p => p.Key)
                        .Where(p => !activeKeys.Contains(p))
                        .ToList();

                    ApplyReturnExtra(notActiveKeys, ref extraItems);
                    ApplyReturnExtra(activeKeys, ref extraItems);
                }
            }
        }

        protected void ApplyReturnExtra(List<int> keys, ref int extraItems)
        {
            List<KeyValuePair<int, Queue<SignalWrapper<TSignal>>>> queues = _itemsQueue
                .Where(p => keys.Contains(p.Key) && p.Value.Count > 0)
                .ToList();

            foreach (KeyValuePair<int, Queue<SignalWrapper<TSignal>>> queue in queues)
            {
                if (extraItems == 0)
                {
                    break;
                }

                List<SignalWrapper<TSignal>> list = queue.Value.ToList();
                int itemsRemoved = 0;

                for (int i = list.Count - 1;
                    i >= 0 && itemsRemoved < extraItems;
                    i--, itemsRemoved++)
                {
                    ApplyResult(list[i], ProcessingResult.ReturnToStorage);
                }

                int itemsLeftCount = list.Count - itemsRemoved;
                list.RemoveRange(itemsLeftCount, itemsRemoved);
                _itemsQueue[queue.Key] = new Queue<SignalWrapper<TSignal>>(list);

                extraItems -= itemsRemoved;
            }
        }        
        
    }
}

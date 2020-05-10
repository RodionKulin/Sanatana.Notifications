using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.Models;
using Sanatana.Notifications.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Flushing.Queues
{
    public abstract class SignalFlushJobBase<TSignal> : FlushJobBase<SignalWrapper<TSignal>>,
        IRegularJob, ISignalFlushJob<TSignal>
    {
        //fields
        protected ISignalQueries<TSignal> _queries;
        protected ITemporaryStorage<TSignal> _temporaryStorage;
        protected TemporaryStorageParameters _temporaryStorageParameters;


        //properties
        /// <summary>
        /// Enable removing items from temporary storage while they are successfully flushed to permanent storage.
        /// </summary>
        public bool IsTemporaryStorageEnabled { get; set; }


        //init
        public SignalFlushJobBase(SenderSettings senderSettings, ITemporaryStorage<TSignal> temporaryStorage, ISignalQueries<TSignal> queries)
            : base(senderSettings)
        {
            _temporaryStorage = temporaryStorage;
            _queries = queries;

            //how to flush items
            new List<FlushAction>
            {
                FlushAction.Insert,
                FlushAction.Update,
                FlushAction.DeleteOne
            }
            .ForEach(action => _flushQueues[action] = new FlushQueue<SignalWrapper<TSignal>>(items => MakeQuery(items, action)));

            //what to do after flushing
            _flushedItemsHandlers.Add(RemovedFlushedFromTempStorage);
        }


        //perform flush
        protected virtual Task MakeQuery(List<SignalWrapper<TSignal>> items, FlushAction action)
        {
            List<TSignal> signals = items.Select(x => x.Signal).ToList();

            if (action == FlushAction.Insert)
            {
                return _queries.Insert(signals);
            }
            if (action == FlushAction.Update)
            {
                return _queries.UpdateSendResults(signals);
            }
            if (action == FlushAction.DeleteOne)
            {
                return _queries.Delete(signals);
            }

            throw new NotImplementedException($"Unknown flush action type {action}");
        }

        protected virtual void RemovedFlushedFromTempStorage(List<SignalWrapper<TSignal>> flushedItems)
        {
            //get item's Ids that were successfully flushed to remove them from temp storage
            List<Guid> tempStorageIds = flushedItems
                .Where(x => x.TempStorageId.HasValue)
                .Select(x => x.TempStorageId.Value)
                .ToList();

            if (IsTemporaryStorageEnabled)
            {
                _temporaryStorage.Delete(_temporaryStorageParameters, tempStorageIds);
            }
        }



        //add to flush queue
        public virtual void Delete(SignalWrapper<TSignal> item)
        {
            if (IsTemporaryStorageEnabled && item.TempStorageId != null)
            {
                _temporaryStorage.Delete(_temporaryStorageParameters, item.TempStorageId.Value);
                item.TempStorageId = null;
            }

            if (item.IsPersistentlyStored)
            {
                _flushQueues[FlushAction.DeleteOne].Queue.Add(item);
            }
        }

        public virtual void Return(SignalWrapper<TSignal> item)
        {
            if (item.IsPersistentlyStored == false)
            {
                //Temp storage item will be deleted after flushing to permanent storage.
                EnqueueItem(item, FlushAction.Insert);
            }
            
            if (item.IsPersistentlyStored == true && item.IsUpdated)
            {
                //Temp storage item will be deleted after flushing to permanent storage.
                //Until then, preserve changes made to TempStorage.
                if (IsTemporaryStorageEnabled && item.TempStorageId != null)
                {
                    _temporaryStorage.Update(_temporaryStorageParameters, item.TempStorageId.Value, item.Signal);
                }

                EnqueueItem(item, FlushAction.Update);
            }
        }
    }
}

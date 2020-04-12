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

            _flushQueues[FlushAction.Insert] = new FlushQueue<SignalWrapper<TSignal>>(signals => MakeQuery(FlushAction.Insert, signals));
            _flushQueues[FlushAction.Update] = new FlushQueue<SignalWrapper<TSignal>>(signals => MakeQuery(FlushAction.Update, signals));
            _flushQueues[FlushAction.DeleteOne] = new FlushQueue<SignalWrapper<TSignal>>(signals => MakeQuery(FlushAction.DeleteOne, signals));
        }


        //perform flush
        protected override List<SignalWrapper<TSignal>> FlushQueues()
        {
            //dequeue items from flush queue
            List<SignalWrapper<TSignal>> flushedItems = base.FlushQueues();

            //get item's Ids that were successfully flushed to remove them from temp storage
            List<Guid> tempStorageIds = flushedItems
                .Where(x => x.TempStorageId.HasValue)
                .Select(x => x.TempStorageId.Value)
                .ToList();

            if (IsTemporaryStorageEnabled)
            {
                _temporaryStorage.Delete(_temporaryStorageParameters, tempStorageIds);
            }

            return flushedItems;
        }

        protected virtual Task MakeQuery(FlushAction action, List<SignalWrapper<TSignal>> items)
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

            throw new NotImplementedException($"Unknow flush action type {action}");
        }


        //add to flush queue
        public virtual void Delete(SignalWrapper<TSignal> item)
        {
            if (IsTemporaryStorageEnabled && item.TempStorageId != null)
            {
                _temporaryStorage.Delete(_temporaryStorageParameters, item.TempStorageId.Value);
                item.TempStorageId = null;
            }

            if (item.IsPermanentlyStored)
            {
                _flushQueues[FlushAction.DeleteOne].Queue.Add(item);
            }
        }

        public virtual void Return(SignalWrapper<TSignal> item)
        {
            if (item.IsPermanentlyStored == false)
            {
                //Temp storage item will be deleted after flushing to permanent storage.
                _flushQueues[FlushAction.Insert].Queue.Add(item);
            }
            
            if (item.IsPermanentlyStored == true && item.IsUpdated)
            {
                //Temp storage item will be deleted after flushing to permanent storage.
                //Until then, preserve changes made to TempStorage.
                if (IsTemporaryStorageEnabled && item.TempStorageId != null)
                {
                    _temporaryStorage.Update(_temporaryStorageParameters, item.TempStorageId.Value, item.Signal);
                }

                _flushQueues[FlushAction.Update].Queue.Add(item);
            }
        }
    }
}

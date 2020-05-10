using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.Locking;
using Sanatana.Notifications.Models;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.Sender;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.Processing.DispatchProcessingCommands
{
    public class CheckConsolidationLockExpirationCommand<TKey> : IDispatchProcessingCommand<TKey>
        where TKey : struct
    {
        //fields
        protected IConsolidationLockTracker<TKey> _consolidationLockTracker;
        protected IConsolidationLockQueries<TKey> _consolidationLockQueries;
        protected SenderSettings _settings;
        protected IDispatchQueue<TKey> _dispatchQueue;


        //properties
        public int Order { get; set; } = 1;



        //ctor
        public CheckConsolidationLockExpirationCommand(IConsolidationLockTracker<TKey> lockTracker,
            IConsolidationLockQueries<TKey> consolidationLockQueries, SenderSettings settings,
            IDispatchQueue<TKey> dispatchQueue)
        {
            _consolidationLockTracker = lockTracker;
            _consolidationLockQueries = consolidationLockQueries;
            _settings = settings;
            _dispatchQueue = dispatchQueue;
        }


        //methods
        public bool Execute(SignalWrapper<SignalDispatch<TKey>> item)
        {
            ConsolidationLock<TKey> groupLock = _consolidationLockTracker.GetOrAddLock(item.Signal);
            if(groupLock == null)
            {
                //other process cleared consolidation locks table in the middle
                //just repeat processing later
                _dispatchQueue.ApplyResult(item, ProcessingResult.Repeat);
                return false;
            }

            bool isLockedByCurrentInstance = groupLock.LockedBy == _settings.LockedByInstanceId;
            if (!isLockedByCurrentInstance)
            {
                //this consolidation is handled by other Sender instance.
                //databaes Signal will be removed after consolidation finished but responsible Sender instance.
                return false;
            }

            bool isConsolidationRoot = EqualityComparer<TKey>.Default.Equals(
                groupLock.ConsolidationRootId, item.Signal.SignalDispatchId);
            if (isConsolidationRoot)
            {
                //lock was created and this dispatch is now consolidation root.
                //can start consolidation.
                return true;
            }

            bool willBeConsolidatedWithCurrentBatch = item.Signal.CreateDateUtc < groupLock.ConsolidationRootSendDateUtc;
            if (willBeConsolidatedWithCurrentBatch)
            {
                //this dispatch will be atteched to ConsolidationRoot.
                //databaes Signal will be removed after consolidation finished.
                return false;
            }

            //next consolidation batch should start after current batch is finished.
            //because need to prevent selecting extra items from previous batch and attach them twice to different consolidation batches.
            //repeat processing item later.
            _dispatchQueue.ApplyResult(item, ProcessingResult.Repeat);
            return false;
        }
                
    }
}

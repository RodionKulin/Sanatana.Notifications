using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.Locking;
using Sanatana.Notifications.Models;
using Sanatana.Notifications.Sender;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.Processing.DispatchProcessingCommands
{
    public class CheckLockExpirationCommand<TKey> : IDispatchProcessingCommand<TKey>
        where TKey : struct
    {
        //fields
        protected ILockTracker<TKey> _lockTracker;
        protected ISignalDispatchQueries<TKey> _dispatchQueries;
        protected SenderSettings _settings;


        //properties
        public int Order { get; set; } = 0;



        //ctor
        public CheckLockExpirationCommand(ILockTracker<TKey> lockTracker, ISignalDispatchQueries<TKey> dispatchQueries,
            SenderSettings settings)
        {
            _lockTracker = lockTracker;
            _dispatchQueries = dispatchQueries;
            _settings = settings;
        }


        //methods
        public bool Execute(SignalWrapper<SignalDispatch<TKey>> item)
        {
            bool isLockingEnabled = _settings.IsDbLockStorageEnabled;
            if (!isLockingEnabled)
            {
                return true;
            }

            if (item.Signal.ShouldBeConsolidated())
            {
                //consolidated items have their own locks for the whole consolidated group of dispatches
                return true;
            }

            bool isLockExpired = _lockTracker.CheckNeedToExtendLock(item.Signal.SignalDispatchId);
            if (!isLockExpired)
            {
                return true;
            }

            //Lock again before sending
            DateTime lockExpirationDate = _lockTracker.GetLockExpirationDate();
            bool lockSet = _dispatchQueries.SetLock(new List<TKey> { item.Signal.SignalDispatchId },
                lockId: _settings.LockedByInstanceId.Value,
                newLockSinceTimeUtc: DateTime.UtcNow,
                existingLockSinceDateUtc: lockExpirationDate)
                .Result;
            if (lockSet)
            {
                return true;
            }

            //If already expired and locked by another Sender instance, do nothing and let another instance to process.
            _lockTracker.ForgetLocks(new List<TKey> { item.Signal.SignalDispatchId });

            //cancel any future steps
            return false;
        }
    }
}

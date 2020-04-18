using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.Notifications.Sender;

namespace Sanatana.Notifications.Locking
{
    /// <summary>
    /// Remember locked signals so they are not queried again from database while being processsed by same Sender instance.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class LockTracker<TKey> : ILockTracker<TKey> 
        where TKey : struct
    {
        //fields
        protected ConcurrentDictionary<TKey, DateTime> _lockStartTime;
        protected SenderSettings _senderSettings;
        protected TimeSpan _expireBeforehandInterval = TimeSpan.FromMinutes(1);


        //ctor
        public LockTracker(SenderSettings senderSettings)
        {
            _lockStartTime = new ConcurrentDictionary<TKey, DateTime>();
            _senderSettings = senderSettings;
        }


        //methods
        public virtual void RememberLock(IEnumerable<TKey> signalIds, DateTime lockStartUtc)
        {
            foreach (TKey signalId in signalIds)
            {
                _lockStartTime.TryAdd(signalId, lockStartUtc);
            }
        }

        /// <summary>
        /// Remove SignalIds, so it is not excluded in database queries. 
        /// </summary>
        /// <param name="signalIds"></param>
        public virtual void ForgetLock(IEnumerable<TKey> signalIds)
        {
            foreach (TKey signalId in signalIds)
            {
                _lockStartTime.TryRemove(signalId, out DateTime _);
            }
        }

        /// <summary>
        /// Get Signal Ids that should be queries again while remaining in Queue.
        /// </summary>
        /// <returns></returns>
        public virtual TKey[] GetLockedIds()
        {
            return _lockStartTime.Keys.ToArray();
        }

        /// <summary>
        /// Signals with expired lock could be selected by another instance of Sender.
        /// To prevent handling multiple times same Signal should set lock again.
        /// </summary>
        /// <param name="signalId"></param>
        /// <returns></returns>
        public virtual bool CheckIsExpired(TKey signalId)
        {
            if (!IsLockingEnabled())
            {
                //It makes sense to disable locking if there is only single instance of Sender, 
                //So expiration of lock should not matter
                return false;
            }

            bool hasValue = _lockStartTime.TryGetValue(signalId, out DateTime lockStartUtc);
            if (!hasValue)
            {
                //should be possible to land here, only if Signal is not stored in database (and should not be locked)
                //because only DatabaseSignalProvider calls RememberLock method
                return false;
            }

            DateTime lockExpirationTime = lockStartUtc
                .Add(_senderSettings.DispatchLockDuration)
                .Subtract(_expireBeforehandInterval); //if lock is close to expiration, update it's value in database, so it is not expired during sending.
            bool isLockExpired = lockExpirationTime > DateTime.UtcNow;
            return isLockExpired;
        }

        public virtual bool IsLockingEnabled()
        {
            return _senderSettings.DatabaseSignalLockId != null;
        }

        /// <summary>
        /// Get date to select locked signals before. 
        /// If database LockDateUtc is before date returned, than it will be locked by different Sender instance and selected for processing.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetLockExpirationDate()
        {
            //where this method is used
            //1.In DatabaseDispatchProvider to SelectLocked all items locked before date
            //2.In DatabaseDispatchProvider to SelectWithSetLock (SelectLocked + SelectLocked) all items locked before date and reselect items from database where Dispatch.LockExpirationUtc < GetLockExpirationDate();
            //3.In CheckLockExpirationCommand to SetLocked to find items to reset LockedBy where Dispatch.LockExpirationUtc < GetLockExpirationDate();

            return DateTime.UtcNow.Subtract(_senderSettings.DispatchLockDuration);            
        }
    }
}

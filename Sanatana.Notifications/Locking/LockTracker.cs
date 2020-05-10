using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.Notifications.Sender;

namespace Sanatana.Notifications.Locking
{
    /// <summary>
    /// Remember currently selected signal, so they are not queried again from database while being processsed by same Sender instance and their lock (if enabled) does not expire.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class LockTracker<TKey> : ILockTracker<TKey> 
        where TKey : struct
    {
        //fields
        protected ConcurrentDictionary<TKey, DateTime> _lockStartTime;
        protected SenderSettings _settings;
        protected TimeSpan _expireBeforehandInterval = NotificationsConstants.DATABASE_LOCK_BEFOREHAND_EXPIRATION;


        //ctor
        public LockTracker(SenderSettings senderSettings)
        {
            _lockStartTime = new ConcurrentDictionary<TKey, DateTime>();
            _settings = senderSettings;
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
        /// Remove Signal Ids from tracker, so it is not excluded in database queries.
        /// Flushing Signal processing results to database happens with a dellay after queue becomes empty. 
        /// Because of this delay need to exlude Signals Ids already processed but not flushed results to database.
        /// </summary>
        /// <param name="signalIds"></param>
        public virtual void ForgetLocks(IEnumerable<TKey> signalIds)
        {
            foreach (TKey signalId in signalIds)
            {
                _lockStartTime.TryRemove(signalId, out DateTime _);
            }
        }

        /// <summary>
        /// Get Signal Ids that should not be queried again while being processed by Sender.
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
        public virtual bool CheckNeedToExtendLock(TKey signalId)
        {
            bool isLockEnabled = _settings.IsDbLockStorageEnabled;
            if (!isLockEnabled)
            {
                //It makes sense to disable locking if there is only single instance of Sender, 
                //So expiration of lock should not matter
                return false;
            }

            bool hasValue = _lockStartTime.TryGetValue(signalId, out DateTime lockStartUtc);
            if (!hasValue)
            {
                //should be possible to land here, only if Signal is not stored in database (so no signal level locking is required)
                //because only DatabaseSignalProvider calls RememberLock method
                return false;
            }

            DateTime lockExpirationTime = lockStartUtc
                .Add(_settings.LockDuration)
                .Subtract(_expireBeforehandInterval); //if lock is close to expiration, update it's value in database, so it is not expired during sending.
            bool isLockExpired = lockExpirationTime < DateTime.UtcNow;
            return isLockExpired;
        }

        /// <summary>
        /// Get date to select locked signals before. 
        /// If database LockSinceUtc is before date returned, than it will be locked by different Sender instance and selected for processing.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetMaxExpiredLockSinceUtc()
        {
            //where this method is used
            //1.From DatabaseDispatchProvider to SelectLocked all items locked before date
            //2.From DatabaseDispatchProvider to SelectWithSetLock (SelectLocked + SelectLocked) all items locked before date and reselect items from database where Dispatch.LockExpirationUtc < GetLockExpirationDate();
            //3.From CheckLockExpirationCommand to SetLocked to find items to reset LockedBy where Dispatch.LockExpirationUtc < GetLockExpirationDate();

            return DateTime.UtcNow.Subtract(_settings.LockDuration);            
        }
    }
}

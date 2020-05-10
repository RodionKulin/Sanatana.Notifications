using RazorEngineCore;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.Sender;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Locking
{
    public class ConsolidationLockTracker<TKey> : IConsolidationLockTracker<TKey>, IRegularJob
        where TKey : struct
    {
        //fields
        protected ConcurrentSet<TKey> _locksCache;
        protected SenderSettings _settings;
        protected IConsolidationLockQueries<TKey> _consolidationLockQueries;
        protected TimeSpan _expireBeforehandInterval = NotificationsConstants.DATABASE_LOCK_BEFOREHAND_EXPIRATION;


        //ctor
        public ConsolidationLockTracker(SenderSettings settings, IConsolidationLockQueries<TKey> consolidationLockQueries)
        {
            _settings = settings;
            _consolidationLockQueries = consolidationLockQueries;
            _locksCache = GetDatabaseLocks();
        }

        protected virtual ConcurrentSet<TKey> GetDatabaseLocks()
        {
            List<ConsolidationLock<TKey>> dbLocks = _settings.IsDbLockStorageEnabled
                ? _consolidationLockQueries.FindAll().Result
                : new List<ConsolidationLock<TKey>>();
            return new ConcurrentSet<TKey>(dbLocks);
        }



        //methods
        protected virtual ConsolidationLock<TKey> ToConsolidationLock(SignalDispatch<TKey> signal)
        {
            return new ConsolidationLock<TKey>
            {
                CategoryId = signal.CategoryId.Value,
                DeliveryType = signal.DeliveryType,
                ReceiverSubscriberId = signal.ReceiverSubscriberId.Value,
                LockedBy = _settings.LockedByInstanceId,
                LockedSinceUtc = DateTime.UtcNow,
                ConsolidationRootId = signal.SignalDispatchId,
                ConsolidationRootSendDateUtc = signal.SendDateUtc
            };
        }

        protected virtual bool CheckIsExpired(ConsolidationLock<TKey> consolidationLock, bool expireBeforehand)
        {
            bool isLockingEnabled = _settings.IsDbLockStorageEnabled;
            if (!isLockingEnabled)
            {
                //if only single Sender instance and database locking is disabled, then can not expire.
                return false;
            }

            DateTime lockExpirationTime = consolidationLock.LockedSinceUtc.Value
                .Add(_settings.LockDuration);
              
            if (expireBeforehand)
            {
                //if lock is close to expiration, update it's value in database, so it is not expired during consolidation.
                //but do not treat as expired locks set by other Sender instances.
                lockExpirationTime = lockExpirationTime.Subtract(_expireBeforehandInterval); 
            }
            
            return lockExpirationTime < DateTime.UtcNow;
        }

        /// <summary>
        /// Track SignalDispatches that are responsible for consolidation.
        /// Required even if single Sender instance is running and no lock is set in database.
        /// Tracking pending consolidations allows to choose only single consolidation root that will attach other dispatches to itself.
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public virtual ConsolidationLock<TKey> GetOrAddLock(SignalDispatch<TKey> signal)
        {
            //Lock for signal group and this signal as ConsolidationRoot
            ConsolidationLock<TKey> groupLock = ToConsolidationLock(signal);
            bool isNewLock = _locksCache.AddIfUnique(groupLock);

            //Lock with same group key can be added in parallel
            //First added lock will be set ConsolidationRootId.
            groupLock = _locksCache.GetMatchingGroup(groupLock);
            bool isConsolidationRoot = EqualityComparer<TKey>.Default.Equals(
                groupLock.ConsolidationRootId, signal.SignalDispatchId);
            if (!isConsolidationRoot)
            {
                //Signal will not start consolidation, but will be attached to ConsolidationRoot
                return groupLock;
            }

            if (!_settings.IsDbLockStorageEnabled)
            {
                return groupLock;
            }

            if (isNewLock)
            {
                bool isDuplicate = _consolidationLockQueries.InsertOneHandleDuplicate(groupLock).Result;
                if (isDuplicate)
                {
                    //get latest value with same group key to cache it
                    //db can return null if some other process will clear ConsolidationLocks collection in the middle
                    groupLock = _consolidationLockQueries.FindExistingMatch(groupLock).Result;
                    _locksCache.ReplaceGroup(groupLock);
                }
            }

            if(groupLock == null)
            {
                return null;
            }

            bool isLockedByCurrentSenderInstance = groupLock.LockedBy == _settings.LockedByInstanceId;
            bool isLockExpired = CheckIsExpired(groupLock, expireBeforehand: isLockedByCurrentSenderInstance);
            if (isLockExpired)
            {
                groupLock.LockedBy = _settings.LockedByInstanceId.Value;
                bool lockExtended = _consolidationLockQueries.ExtendLockTime(groupLock).Result;
                if (!lockExtended)
                {
                    //get latest value with same group key to store it in cache
                    //db can return null if some other process will clear ConsolidationLocks collection in the middle
                    groupLock = _consolidationLockQueries.FindExistingMatch(groupLock).Result;
                }
                //update LockedSinceUtc or LockedSinceUtc with LockedBy
                _locksCache.ReplaceGroup(groupLock);
            }

            return groupLock;
        }

        /// <summary>
        /// Get Consolidation locks describing range of DispatchSignals that should not be queried again while being processed by Sender.
        /// </summary>
        /// <returns></returns>
        public virtual ConsolidationLock<TKey>[] GetLockedGroups()
        {
            return _locksCache.GetAll().ToArray();
        }

        /// <summary>
        /// Remove ConsolidationLocks, so Consolidation groups are not excluded from database select queries.
        /// </summary>
        /// <param name="signals"></param>
        public virtual void ForgetLocks(IEnumerable<SignalDispatch<TKey>> signals)
        {
            ConsolidationLock<TKey>[] groupLocks = signals.Select(ToConsolidationLock).ToArray();
            if (_settings.IsDbLockStorageEnabled)
            {
                _consolidationLockQueries.Delete(groupLocks).Wait();
            }

            _locksCache.Remove(groupLocks);
        }



        //IRegularJob
        public void Tick()
        {
            ForgetExpiredLocks();
        }

        public void Flush()
        {
        }

        public virtual void ForgetExpiredLocks()
        {
            if (!_settings.IsDbLockStorageEnabled)
            {
                //there is no expiration when only single Sender instance is working
                return;
            }

            //if lock is expired, then likely already handled by other Sender instance
            //or other Sender instance is down and can take it's consolidation groups to process
            ConsolidationLock<TKey>[] expiredLocks = _locksCache.GetAll()
                .Where(x => x.LockedBy != _settings.LockedByInstanceId)
                .Where(x => CheckIsExpired(x, expireBeforehand: false))
                .ToArray();
            _locksCache.Remove(expiredLocks);
        }
    }
}

using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;

namespace Sanatana.Notifications.Locking
{
    public interface IConsolidationLockTracker<TKey>
        where TKey : struct
    {
        ConsolidationLock<TKey>[] GetLockedGroups();
        ConsolidationLock<TKey> GetOrAddLock(SignalDispatch<TKey> signal);
        void ForgetLocks(IEnumerable<SignalDispatch<TKey>> signals);
        void ForgetExpiredLocks();
    }
}
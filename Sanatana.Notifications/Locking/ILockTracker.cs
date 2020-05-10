using System;
using System.Collections.Generic;

namespace Sanatana.Notifications.Locking
{
    public interface ILockTracker<TKey> 
        where TKey : struct
    {
        bool CheckNeedToExtendLock(TKey signalId);
        void ForgetLocks(IEnumerable<TKey> signalIds);
        TKey[] GetLockedIds();
        void RememberLock(IEnumerable<TKey> signalIds, DateTime lockStartUtc);
        DateTime GetLockExpirationDate();
    }
}
using System;
using System.Collections.Generic;

namespace Sanatana.Notifications.Locking
{
    public interface ILockTracker<TKey> 
        where TKey : struct
    {
        bool IsLockingEnabled();
        bool CheckIsExpired(TKey signalId);
        void ForgetLock(IEnumerable<TKey> signalIds);
        TKey[] GetLockedIds();
        void RememberLock(IEnumerable<TKey> signalIds, DateTime lockStartUtc);
        DateTime GetLockExpirationDate();
    }
}
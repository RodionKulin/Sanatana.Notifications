using System;

namespace Sanatana.Notifications.DispatchHandling.Limits.JournalStorage
{
    public interface IJournalStorage : IDisposable
    {
        void CleanJournal(TimeSpan deleteBeforePeriod);
        int GetSendingCapacity(global::System.Collections.Generic.List<LimitedPeriod> periods);
        DateTime? GetLimitsEndTimeUtc(global::System.Collections.Generic.List<LimitedPeriod> periods);
        void InsertTime();
    }
}

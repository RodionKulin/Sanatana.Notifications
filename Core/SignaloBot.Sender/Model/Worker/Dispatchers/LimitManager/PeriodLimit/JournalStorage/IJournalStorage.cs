using System;

namespace SignaloBot.Sender.Senders.LimitManager.JournalStorage
{
    public interface IJournalStorage : IDisposable
    {
        void CleanJournal(TimeSpan deleteBeforePeriod);
        int GetSendingCapacity(global::System.Collections.Generic.List<LimitedPeriod> periods);
        DateTime GetLimitsEndTimeUtc(global::System.Collections.Generic.List<LimitedPeriod> periods);
        void InsertTime();
    }
}

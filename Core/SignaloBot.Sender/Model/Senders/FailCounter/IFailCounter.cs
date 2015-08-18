using System;

namespace SignaloBot.Sender.Senders.FailCounter
{
    public interface IFailCounter
    {
        bool IsPenaltyActive { get; }

        void Success();
        void Fail();
    }
}

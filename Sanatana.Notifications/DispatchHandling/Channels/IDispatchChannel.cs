using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.Models;
using System;

namespace Sanatana.Notifications.DispatchHandling.Channels
{
    public interface IDispatchChannel<TKey> : IDisposable
        where TKey : struct
    {
        int AvailableLimitCapacity { get; }
        int DeliveryType { get; }
        bool IsActive { get; }

        DispatcherAvailability CheckAvailability();
        void CountAvailabilityCheck(DispatcherAvailability availability);
        void CountSendAttempt(SignalDispatch<TKey> dispatch, ProcessingResult result, DispatcherAvailability availability);
        ProcessingResult Send(SignalDispatch<TKey> dispatch);
        void SetLimitsCapacity();
        void SetRestrictionsDuration();
    }
}
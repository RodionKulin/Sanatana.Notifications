using Sanatana.Timers;
using Sanatana.Timers.Switchables;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.EventTracking;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.SignalProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sanatana.Notifications.SignalProviders.Interfaces;

namespace Sanatana.Notifications.Sender
{
    public class Sender<TKey> : IDisposable, ISender
        where TKey : struct
    {
        //fields
        protected SenderState<TKey> _hubState;
        protected IEnumerable<IRegularJob> _regularJobs;
        protected IEventQueue<TKey> _eventQueue;
        protected IDispatchQueue<TKey> _dispatchQueue;
        protected List<ISignalProviderControl> _signalEndpoints;
        protected List<NonReentrantTimer> _regularJobTimers;
        protected NonReentrantTimer _stopMonitorTimer;
        protected ManualResetEventSlim _stopEventHandle;
        protected TimeSpan _stopTimeout;


        //properties
        public virtual SwitchState State
        {
            get
            {
                return _hubState.State;
            }
        }


        //init
        public Sender(SenderState<TKey> hubState
            , IEventQueue<TKey> eventQueue, IDispatchQueue<TKey> dispatchQueue
            , IEnumerable<IRegularJob> regularJobs, IEnumerable<ISignalProviderControl> signalEndpoints)
        {
            _hubState = hubState;
            _regularJobs = regularJobs;
            _eventQueue = eventQueue;
            _dispatchQueue = dispatchQueue;
            _signalEndpoints = signalEndpoints.ToList();

            _regularJobTimers = _regularJobs.Select(x => new NonReentrantTimer((Action)x.Tick
                , NotificationsConstants.REGULAR_JOB_TICK_INTERVAL, intervalFromCallbackStarted: false))
                .ToList();
            _stopMonitorTimer = new NonReentrantTimer(StopTimers
                , NotificationsConstants.REGULAR_JOB_TICK_INTERVAL, intervalFromCallbackStarted: false);
            _stopEventHandle = new ManualResetEventSlim(false);
        }


        //start
        public virtual void Start()
        {
            if (_hubState.State == SwitchState.Started)
            {
                return;
            }
            _hubState.State = SwitchState.Started;

            _eventQueue.RestoreFromTemporaryStorage();
            _dispatchQueue.RestoreFromTemporaryStorage();

            _signalEndpoints.ForEach(x => x.Start());
            _regularJobTimers.ForEach(x => x.Start());
        }


        //stop
        public virtual void Stop(bool blockThread, TimeSpan? timeout = null)
        {
            if (_hubState.State != SwitchState.Started)
            {
                return;
            }

            _hubState.State = SwitchState.Stopping;
            DateTime stopStartedTime = DateTime.UtcNow;

            if (blockThread)
            {
                _stopEventHandle.Reset();
            }

            _regularJobTimers.ForEach(x => x.Stop());
            _stopMonitorTimer.Start();

            if (blockThread)
            {
                if (timeout == null)
                {
                    _stopEventHandle.Wait();
                }
                else
                {
                    TimeSpan timePassed = DateTime.UtcNow - stopStartedTime;
                    TimeSpan timeLeftUntilTimeout = timeout.Value - timePassed;
                    timeLeftUntilTimeout = timeLeftUntilTimeout > TimeSpan.Zero ? timeLeftUntilTimeout : TimeSpan.Zero;
                    _stopEventHandle.Wait(timeLeftUntilTimeout);
                } 
            }

        }

        protected virtual bool StopTimers()
        {
            bool isProcessingJob = _regularJobTimers.Any(x => x.IsProcessingCallback);
            if (isProcessingJob)
            {
                return true;
            }

            foreach (ISignalProviderControl endpoint in _signalEndpoints)
            {
                endpoint.Stop(_stopTimeout);
            }
            foreach (IRegularJob regularJob in _regularJobs)
            {
                regularJob.Flush();
            }

            _hubState.State = SwitchState.Stopped;
            _stopEventHandle.Set();

            return false;
        }


        //dispose
        public virtual void Dispose()
        {
            _regularJobTimers.ForEach(x => x.Dispose());
            _stopMonitorTimer.Dispose();
            _stopEventHandle.Dispose();
        }
    }
}

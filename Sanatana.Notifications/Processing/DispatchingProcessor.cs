using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Dispatching.Channels;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.Resources;
using Sanatana.Notifications.Dispatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Entities;
using Microsoft.Extensions.Logging;
using Sanatana.Timers.Switchables;
using Sanatana.Notifications.Sender;

namespace Sanatana.Notifications.Processing
{
    public class DispatchingProcessor<TKey> : ProcessorBase<TKey>, IRegularJob
        where TKey : struct
    {
        //fields
        protected SenderState<TKey> _hubState;
        protected IMonitor<TKey> _eventSink;
        protected IDispatchChannelRegistry<TKey> _channelRegistry;
        protected IDispatchQueue<TKey> _dispatchQueue;


        //init
        public DispatchingProcessor(SenderState<TKey> hubState, IDispatchQueue<TKey> dispatchQueue
            , IDispatchChannelRegistry<TKey> channelRegistry, IMonitor<TKey> eventSink
            , ILogger logger, SenderSettings senderSettings)
            : base(logger)
        {
            _hubState = hubState;
            _dispatchQueue = dispatchQueue;
            _channelRegistry = channelRegistry;
            _eventSink = eventSink;

            MaxParallelItems = senderSettings.MaxParallelDispatchesProcessed;
        }



        //IRegularJob methods
        public virtual void Tick()
        {
            if (CanContinue(false) == false)
            {
                return;
            }

            List<DispatchChannel<TKey>> dispatchChannels = _channelRegistry.GetAll();
            foreach (DispatchChannel<TKey> dispatcher in dispatchChannels)
            {
                dispatcher.SetLimitsCapacity();
            }

            DequeueAll();

            foreach (DispatchChannel<TKey> dispatcher in dispatchChannels)
            {
                dispatcher.SetRestrictionsDuration();
            }
        }

        public virtual void Flush()
        {
        }


        //processing methods
        protected virtual void DequeueAll()
        {
            while (CanContinue())
            {
                SignalWrapper<SignalDispatch<TKey>> item = _dispatchQueue.DequeueNext();
                if (item == null)
                {
                    break;
                }

                StartNextTask(() => ProcessSignal(item, _dispatchQueue));
            }
            WaitForCompletion();
        }

        /// <summary>
        /// Check if can process dispatches. Checking limit capacity is done only once while queue is still not empty.
        /// </summary>
        /// <param name="checkLimitCapacity"></param>
        /// <returns></returns>
        protected virtual bool CanContinue(bool checkLimitCapacity)
        {
            List<int> activeDeliveryTypes = _channelRegistry.GetActiveDeliveryTypes(checkLimitCapacity);
            bool isQueueEmpty = _dispatchQueue.CheckIsEmpty(activeDeliveryTypes);

            return !isQueueEmpty && _hubState.State == SwitchState.Started;
        }

        protected virtual bool CanContinue()
        {
            return CanContinue(true);
        }

        protected void ProcessSignal(SignalWrapper<SignalDispatch<TKey>> item, IDispatchQueue<TKey> queue)
        {
            DispatchChannel<TKey> dispatchChannel = _channelRegistry.Match(item.Signal);
            
            ProcessingResult sendResult = ProcessingResult.NoHandlerFound;
            DispatcherAvailability dispatcherAvailability = DispatcherAvailability.NotAvailable;
            TimeSpan sendDuration = TimeSpan.FromSeconds(0);

            if (dispatchChannel != null)
            {
                Stopwatch sendTimer = Stopwatch.StartNew();
                try
                {
                    sendResult = dispatchChannel.Dispatcher.Send(item.Signal).Result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, null);
                    sendResult = ProcessingResult.Fail;
                }
                sendDuration = sendTimer.Elapsed;
            }

            dispatcherAvailability = dispatchChannel.ApplyResult(sendResult, item.Signal, _logger);
            if (sendResult == ProcessingResult.Fail && dispatcherAvailability == DispatcherAvailability.NotAvailable)
            {
                sendResult = ProcessingResult.Repeat;
            }

            queue.ApplyResult(item, sendResult);
            _eventSink.DispatchSent(item.Signal, sendDuration, sendResult, dispatcherAvailability);
        }


    }
}

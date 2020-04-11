using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DispatchHandling.Channels;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.Resources;
using Sanatana.Notifications.DispatchHandling;
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
using Sanatana.Notifications.Models;
using Sanatana.Notifications.Processing.DispatchProcessingCommands;

namespace Sanatana.Notifications.Processing
{
    public class DispatchProcessor<TKey> : ProcessorBase<TKey>, IRegularJob, IDispatchProcessor
        where TKey : struct
    {
        //fields
        protected SenderState<TKey> _hubState;
        protected IDispatchChannelRegistry<TKey> _channelRegistry;
        protected IDispatchQueue<TKey> _dispatchQueue;
        protected IDispatchProcessingCommand<TKey>[] _processingCommands;


        //init
        public DispatchProcessor(SenderState<TKey> hubState, IDispatchQueue<TKey> dispatchQueue,
            ILogger logger, SenderSettings senderSettings,IDispatchChannelRegistry<TKey> channelRegistry,
            IEnumerable<IDispatchProcessingCommand<TKey>> processingCommands)
            : base(logger)
        {
            _hubState = hubState;
            _dispatchQueue = dispatchQueue;
            _channelRegistry = channelRegistry;
            _processingCommands = processingCommands
                .OrderBy(x => x.Order)
                .ToArray();

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

                StartNextTask(() => ProcessSignal(item));
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

        protected void ProcessSignal(SignalWrapper<SignalDispatch<TKey>> item)
        {
            foreach (IDispatchProcessingCommand<TKey> command in _processingCommands)
            {
                bool completed = command.Execute(item).Result;
                if (!completed)
                {
                    break;
                }
            }
        }

    }
}

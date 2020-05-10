using Microsoft.Extensions.Logging;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DispatchHandling;
using Sanatana.Notifications.DispatchHandling.Channels;
using Sanatana.Notifications.Models;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Processing.DispatchProcessingCommands
{
    public class SendDispatchCommand<TKey> : IDispatchProcessingCommand<TKey>
        where TKey : struct
    {
        //fields
        protected IDispatchChannelRegistry<TKey> _channelRegistry;
        protected ILogger _logger;
        protected IDispatchQueue<TKey> _dispatchQueue;
        protected IMonitor<TKey> _monitor;


        //properties
        public int Order { get; set; } = 3;


        //ctor
        public SendDispatchCommand(IDispatchChannelRegistry<TKey> channelRegistry, ILogger logger,
            IDispatchQueue<TKey> dispatchQueue, IMonitor<TKey> monitor)
        {
            _channelRegistry = channelRegistry;
            _logger = logger;
            _dispatchQueue = dispatchQueue;
            _monitor = monitor;
        }


        //methods
        public virtual bool Execute(SignalWrapper<SignalDispatch<TKey>> item)
        {
            //find Dispatcher by deliveryType
            IDispatchChannel<TKey> channel = _channelRegistry.Match(item.Signal);
            if (channel == null)
            {
                _dispatchQueue.ApplyResult(item, ProcessingResult.NoHandlerFound);
                return false;
            }

            //send with dispatcher
            ProcessingResult sendResult = ProcessingResult.Fail;
            Stopwatch sendTimer = Stopwatch.StartNew();
            try
            {
                sendResult = channel.Send(item.Signal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }
            sendTimer.Stop();

            //check dispatcher availability
            DispatcherAvailability availability = CheckAvailability(channel, sendResult);
            if (sendResult == ProcessingResult.Fail && availability == DispatcherAvailability.NotAvailable)
            {
                sendResult = ProcessingResult.Repeat;
            }

            _monitor.DispatchSent(item.Signal, sendResult, sendTimer.Elapsed);
            channel.CountSendAttempt(item.Signal, sendResult, availability);
            _dispatchQueue.ApplyResult(item, sendResult);
            return sendResult == ProcessingResult.Success;
        }

        protected virtual DispatcherAvailability CheckAvailability(IDispatchChannel<TKey> channel, ProcessingResult sendResult)
        {
            if(sendResult != ProcessingResult.Fail)
            {
                return DispatcherAvailability.NotChecked;
            }

            DispatcherAvailability availability;
            try
            {
                availability = channel.CheckAvailability();
            }
            catch (Exception ex)
            {
                availability = DispatcherAvailability.NotAvailable;
                _logger.LogError(ex, null);
            }

            channel.CountAvailabilityCheck(availability);
            _monitor.DispatchChannelAvailabilityChecked(channel, availability);

            return availability;
        }
    }
}

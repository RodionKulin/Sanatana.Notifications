using Microsoft.Extensions.Logging;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DispatchHandling;
using Sanatana.Notifications.DispatchHandling.Channels;
using Sanatana.Notifications.Models;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Queues;
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
        public int Order { get; set; }


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
        public virtual async Task<bool> Execute(SignalWrapper<SignalDispatch<TKey>> item)
        {
            //find Dispatcher by deliveryType key
            DispatchChannel<TKey> dispatchChannel = _channelRegistry.Match(item.Signal);
            if (dispatchChannel == null)
            {
                _dispatchQueue.ApplyResult(item, ProcessingResult.NoHandlerFound);
                _monitor.DispatchSent(item.Signal, TimeSpan.Zero, ProcessingResult.NoHandlerFound, DispatcherAvailability.NotAvailable);
                return false;
            }

            //send
            ProcessingResult sendResult = ProcessingResult.Fail;
            Stopwatch sendTimer = Stopwatch.StartNew();
            bool successfulySent = false;
            try
            {
                sendResult = await dispatchChannel.Dispatcher.Send(item.Signal).ConfigureAwait(false);
                successfulySent = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }
            sendTimer.Stop();

            //check availability if failed and increment counters
            DispatcherAvailability availability = dispatchChannel.ApplyResult(sendResult, item.Signal, _logger);
            if (sendResult == ProcessingResult.Fail && availability == DispatcherAvailability.NotAvailable)
            {
                sendResult = ProcessingResult.Repeat;
            }

            //store results
            _dispatchQueue.ApplyResult(item, sendResult);
            _monitor.DispatchSent(item.Signal, sendTimer.Elapsed, sendResult, availability);
            return successfulySent;
        }

    }
}

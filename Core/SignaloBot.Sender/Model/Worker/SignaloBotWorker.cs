using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL;
using SignaloBot.Sender;
using SignaloBot.Sender.Processors;
using System.Threading;

namespace SignaloBot.Sender
{
    internal class SignaloBotWorker<TKey> : IDisposable
        where TKey : struct
    {
        //поля
        private enum StopStage { Service, FlushQueue, Processors, Finish };
        private StopStage _stopState;

        private SignaloBotHub<TKey> _context;
        private NonReentrantTimer _eventQueueTimer;
        private NonReentrantTimer _dispatchQueueTimer;
        private NonReentrantTimer _eventProcessorTimer;
        private NonReentrantTimer _dispatchProcessorTimer;
        private NonReentrantTimer _stopMonitorTimer;
        private EventProcessor<TKey> _eventProcessor;
        private DispatchProcessor<TKey> _dispatchProcessor;
        private ManualResetEventSlim _stopEventHandle;


        //инициализация
        public SignaloBotWorker(SignaloBotHub<TKey> context)
        {
            _eventProcessor = new EventProcessor<TKey>(context, context.EventQueues);
            _dispatchProcessor = new DispatchProcessor<TKey>(context, context.DispatchQueues);

            _eventQueueTimer = new NonReentrantTimer(OnEventQueuesTimerTick
                , SenderConstants.QUEUES_TICK_INTERVAL, intervalFromCallbackStarted: false);
            _dispatchQueueTimer = new NonReentrantTimer(OnDispatchQueuesTimerTick
                , SenderConstants.QUEUES_TICK_INTERVAL, intervalFromCallbackStarted: false);
            _eventProcessorTimer = new NonReentrantTimer(_eventProcessor.OnTick
                , SenderConstants.PROCESSORS_TICK_INTERVAL, intervalFromCallbackStarted: false);
            _dispatchProcessorTimer = new NonReentrantTimer(_dispatchProcessor.OnTick
                , SenderConstants.PROCESSORS_TICK_INTERVAL, intervalFromCallbackStarted: false);

            _stopMonitorTimer = new NonReentrantTimer(CheckStopStage
                , SenderConstants.STOPPING_MONITOR_TICK_INTERVAL, intervalFromCallbackStarted: false);

            _context = context;
        }

        

        //start
        public void Start()
        {
            _eventQueueTimer.Start();
            _dispatchQueueTimer.Start();
            _eventProcessorTimer.Start();
            _dispatchProcessorTimer.Start();
        }

        private bool OnEventQueuesTimerTick()
        {
            foreach (IEventQueue<TKey> eventQueue in _context.EventQueues)
            {
                if (_context.State == SwitchState.Started)
                {
                    eventQueue.OnTick(_context.StatisticsCollector);
                }
            }

            return _context.State == SwitchState.Started;
        }

        private bool OnDispatchQueuesTimerTick()
        {
            List<int> activeDeliveryTypes = _context.GetActiveSendersTypes(false);
            foreach (IDispatchQueue<TKey> dispatchQueue in _context.DispatchQueues)
            {
                if(_context.State == SwitchState.Started)
                {
                    dispatchQueue.OnTick(activeDeliveryTypes, _context.StatisticsCollector);                    
                }
            }
            
            return _context.State == SwitchState.Started;
        }

        
        //stop
        public void Stop(bool blockThread, TimeSpan? timeout)
        {
            if (_context.State != SwitchState.Started)
            {
                return;
            }

            _context.State = SwitchState.Stopping;
            _stopState = StopStage.Service;

            if (_context.ServiceHost != null)
            {
                TimeSpan? serviceTimeout = timeout?.Multiply(0.5);
                _context.ServiceHost.Stop(timeout);
                _stopState = StopStage.FlushQueue;
            }

            if (blockThread)
            {
                _stopEventHandle = _stopEventHandle ?? new ManualResetEventSlim(false);
                _stopEventHandle.Reset();
            }

            _stopMonitorTimer.Start();

            if (blockThread)
            {
                if (timeout == null)
                    _stopEventHandle.Wait();
                else
                    _stopEventHandle.Wait(timeout.Value);
            }

        }

        private bool CheckStopStage()
        {
            if (_stopState == StopStage.FlushQueue)
            {
                ReturnQueueSignals();
                _stopState = StopStage.Processors;
            }

            if (_stopState == StopStage.Processors)
            {
                bool processorsStopped = !_eventProcessorTimer.IsStarted
                    && !_dispatchProcessorTimer.IsStarted;

                if (processorsStopped)
                {
                    _stopState = StopStage.Finish;
                }
            }

            if (_stopState == StopStage.Finish)
            {
                ReturnQueueSignals();

                _stopState = StopStage.Service;
                _context.State = SwitchState.Stopped;

                if(_stopEventHandle != null)
                {
                    _stopEventHandle.Set();
                }

                return false;
            }

            return true;
        }

        private void ReturnQueueSignals()
        {
            foreach (IEventQueue<TKey> eventQueue in _context.EventQueues)
            {
                eventQueue.ReturnAll();
            }
            foreach (IDispatchQueue<TKey> dispatchQueue in _context.DispatchQueues)
            {
                dispatchQueue.ReturnAll();
            }
        }


        //IDisposable
        public void Dispose()
        {
            _eventQueueTimer.Dispose();
            _dispatchQueueTimer.Dispose();
            _eventProcessorTimer.Dispose();
            _dispatchProcessorTimer.Dispose();
            
            if (_stopEventHandle != null)
                _stopEventHandle.Dispose();
        }

               
    }
}

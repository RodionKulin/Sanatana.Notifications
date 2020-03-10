using Sanatana.Notifications.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;
using System.Collections.ObjectModel;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.Sender;

namespace Sanatana.Notifications.SignalProviders.WCF
{
    public class SignalServiceInstanceProvider<TKey> : IServiceBehavior, IInstanceProvider
        where TKey : struct
    {
        private IEventQueue<TKey> _eventQueues;
        private IDispatchQueue<TKey> _dispatchQueues;
        private IMonitor<TKey> _monitor;
        private ISignalEventQueries<TKey> _eventQueries;
        private ISignalDispatchQueries<TKey> _dispatchQueries;
        private SenderSettings _senderSettings;


        //init
        public SignalServiceInstanceProvider(IEventQueue<TKey> eventQueues, IDispatchQueue<TKey> dispatchQueues, 
            IMonitor<TKey> eventSink, ISignalEventQueries<TKey> eventQueries,
            ISignalDispatchQueries<TKey> dispatchQueries, SenderSettings senderSettings)
        {
            _eventQueues = eventQueues;
            _dispatchQueues = dispatchQueues;
            _monitor = eventSink;
            _eventQueries = eventQueries;
            _dispatchQueries = dispatchQueries;
            _senderSettings = senderSettings;
        }


        //methods
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.GetInstance(instanceContext);
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return new SignalService<TKey>(_eventQueues, _dispatchQueues, _monitor,
               _eventQueries, _dispatchQueries, _senderSettings);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }


        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
            foreach (EndpointDispatcher ed in cd.Endpoints)
            {
                ed.DispatchRuntime.InstanceProvider = this;
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

    }
}
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
using Sanatana.Notifications.EventTracking;
using Sanatana.Notifications.DAL;

namespace Sanatana.Notifications.SignalProviders.WCF
{
    public class SignalServiceInstanceProvider<TKey> : IServiceBehavior, IInstanceProvider
        where TKey : struct
    {
        private IEventQueue<TKey> _eventQueues;
        private IDispatchQueue<TKey> _dispatchQueues;
        private IEventTracker<TKey> _eventSink;


        //init
        public SignalServiceInstanceProvider(IEventQueue<TKey> eventQueues
            , IDispatchQueue<TKey> dispatchQueues, IEventTracker<TKey> eventSink)
        {
            _eventQueues = eventQueues;
            _dispatchQueues = dispatchQueues;
            _eventSink = eventSink;
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.GetInstance(instanceContext);
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return new SignalService<TKey>(_eventQueues, _dispatchQueues, _eventSink);
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
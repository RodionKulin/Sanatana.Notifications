using SignaloBot.Sender.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;
using System.Collections.ObjectModel;
using SignaloBot.Sender.Statistics;
using SignaloBot.DAL;

namespace SignaloBot.Sender.Service
{
    public class SignalServiceInstanceProvider<TKey> : IServiceBehavior, IInstanceProvider
        where TKey : struct
    {
        private readonly IEventQueue<TKey> _queue;
        private readonly IStatisticsCollector<TKey> _stats;


        //инициализация
        public SignalServiceInstanceProvider(IEventQueue<TKey> queue, IStatisticsCollector<TKey> stats)
        {
            _queue = queue;
            _stats = stats;
        }

        #region IInstanceProvider Members

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.GetInstance(instanceContext);
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return new SignalService<TKey>(_queue, _stats);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        #endregion


        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher ed in cd.Endpoints)
                {
                    ed.DispatchRuntime.InstanceProvider = this;
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

    }
}
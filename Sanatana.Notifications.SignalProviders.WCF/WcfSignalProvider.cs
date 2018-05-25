using Sanatana.Notifications.Queues;
using Sanatana.Notifications.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace Sanatana.Notifications.SignalProviders.WCF
{
    public class WcfSignalProvider<TKey> : ISignalProviderControl
        where TKey : struct
    {
        //fields
        protected ServiceHost _host;
        protected SignalServiceInstanceProvider<TKey> _instanceProvider;


        //init
        public WcfSignalProvider(SignalServiceInstanceProvider<TKey> instanceProvider)
        {
            _instanceProvider = instanceProvider;
        }


        //methods
        public virtual void Start()
        {
            bool hostIsCreated = _host != null;
            Func<bool> isClosed = () => _host.State == CommunicationState.Closed
                || _host.State == CommunicationState.Closing
                || _host.State == CommunicationState.Faulted;
            if (hostIsCreated == true && isClosed() == false)
            {
                return;
            }

            _host = new ServiceHost(typeof(SignalService<TKey>));
            _host.Description.Behaviors.Add(_instanceProvider);
            _host.Open();
        }

        public virtual void Stop(TimeSpan? timeout)
        {
            if(_host == null)
            {
                return;
            }

            try
            {
                if (timeout == null)
                    _host.Close();
                else
                    _host.Close(timeout.Value);
            }
            catch
            {
                _host.Abort();
            }
        }
        
    }
}
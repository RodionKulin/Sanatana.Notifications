using Common.Utility;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace SignaloBot.Sender.Service
{
    public class SignalServiceSelfHost<TKey>
        where TKey : struct
    {
        //поля
        private ICommonLogger _logger;
        private ServiceHost _host;
        private SignalServiceInstanceProvider<TKey> _instanceProvider;


        //инициализация
        public SignalServiceSelfHost(ICommonLogger logger
            , SignalServiceInstanceProvider<TKey> instanceProvider)
        {
            _logger = logger;
            _instanceProvider = instanceProvider;
        }


        //методы
        public void Start()
        {
            if (_host != null && _host.State != CommunicationState.Closed
                && _host.State != CommunicationState.Closing && _host.State != CommunicationState.Faulted)
            {
                return;
            }

            try
            {
                _host = new ServiceHost(typeof(SignalService<TKey>));
                _host.Description.Behaviors.Add(_instanceProvider);
                _host.Open();
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.Exception(ex);

                _host.Abort();
            }
        }

        public void Stop(TimeSpan? timeout)
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
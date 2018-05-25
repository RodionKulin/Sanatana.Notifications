using Microsoft.Extensions.Logging;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DeliveryTypes.Email;
using Sanatana.Notifications.Dispatching;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DeliveryTypes.Http
{
    public class HttpDispatcher<TKey> : IDispatcher<TKey>
        where TKey : struct
    {
        //fields
        protected ILogger _logger;


        //init
        public HttpDispatcher(ILogger logger)
        {
            _logger = logger;
        }



        //methods
        public virtual async Task<ProcessingResult> Send(SignalDispatch<TKey> item)
        {
            if ((item is HttpDispatch<TKey>) == false)
            {
                _logger.LogError(SenderInternalMessages.Dispatcher_WrongInputType
                    , item.GetType(), GetType(), typeof(EmailDispatch<TKey>));
                return ProcessingResult.Fail;
            }
            HttpDispatch<TKey> httpDispatch = item as HttpDispatch<TKey>;

            HttpRequestMessage request = BuildRequest(httpDispatch);
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.SendAsync(request);
                HandleResponse(item, response);
            }

            return ProcessingResult.Success;
        }

        protected virtual HttpRequestMessage BuildRequest(HttpDispatch<TKey> httpDispatch)
        {
            var request = new HttpRequestMessage
            {
                Method = httpDispatch.HttpMethod,
                RequestUri = new Uri(httpDispatch.Url)
            };

            if (httpDispatch.Content != null)
            {
                request.Content = new StringContent(httpDispatch.Content);
            }

            if (httpDispatch.Headers != null)
            {
                foreach (KeyValuePair<string, string> header in httpDispatch.Headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            return request;
        }

        protected virtual void HandleResponse(SignalDispatch<TKey> item, HttpResponseMessage response)
        {

        }

        public virtual Task<DispatcherAvailability> CheckAvailability()
        {
            return Task.FromResult(DispatcherAvailability.Available);
        }

        public virtual void Dispose()
        {
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace SignaloBot.NDR.Host
{
    public class NDRWebApiHost : IDisposable
    {
        //поля
        protected HttpSelfHostServer _server;


        //методы
        public void Open(string controllerName, int? port, string path)
        {
            string domain = "http://localhost";
            if (port != null)
                domain += ":" + port.Value;
            var config = new HttpSelfHostConfiguration(domain);

            config.Formatters.Insert(0, new TextMediaTypeFormatter());
            //config.MessageHandlers.Add(new UnsupportedMediaTypeConnegHandler());

            config.Routes.MapHttpRoute(
                "API Default", path,
                new
                {
                    controller = controllerName,
                    action = "Handle"
                });

            _server = new HttpSelfHostServer(config);
            _server.OpenAsync().Wait();
        }

        public void Close()
        {
            if (_server != null)
            {
                _server.CloseAsync().Wait();
            }
        }
        
        public void Dispose()
        {
            if (_server != null)
            {
                Close();

                try
                {
                    _server.Dispose();
                }
                catch 
                {

                }
            }
        }
    }
}

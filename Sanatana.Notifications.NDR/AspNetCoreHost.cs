using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Sanatana.Timers.Switchables;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Sanatana.Notifications.NDR
{
    public class AspNetCoreHost
    {
        //fields
        protected IWebHost _host;
        protected SwitchState _state = SwitchState.Stopped;
        protected INdrHandler _ndrHandler;


        //properties
        public virtual SwitchState State
        {
            get
            {
                return _state;
            }
        }


        //init
        public AspNetCoreHost(INdrHandler ndrHandler)
        {
            _host = CreateWebHost();
            _ndrHandler = ndrHandler;
        }


        //methods
        protected virtual IWebHost CreateWebHost()
        {
            string[] args = new string[0];
            string pathToExe = Process.GetCurrentProcess().MainModule.FileName;
            string pathToContentRoot = Path.GetDirectoryName(pathToExe);

            IWebHost host = WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(pathToContentRoot)
                .UseKestrel(options => ConfigureKestrel(options))
                .ConfigureServices(services => ConfigureServices(services))
                .Configure(app => Configure(app))
                .Build();

            return host;
        }

        protected virtual void ConfigureKestrel(KestrelServerOptions options)
        {
            options.Listen(IPAddress.Loopback, 5000);
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.Add(new ServiceDescriptor(typeof(INdrHandler), _ndrHandler));
        }

        protected virtual void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }

        public virtual void Start()
        {
            if (_state == SwitchState.Started)
            {
                return;
            }
            _state = SwitchState.Started;

            _host.Start();
        }

        public virtual void Stop()
        {
            if (_state != SwitchState.Started)
            {
                return;
            }
            _state = SwitchState.Stopped;

            try
            {
                _host.StopAsync().GetAwaiter().GetResult();
            }
            finally
            {
                _host.Dispose();
            }
        }
    }
}

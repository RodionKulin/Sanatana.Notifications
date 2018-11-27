using Autofac;
using Sanatana.Notifications.SignalProviders;
using Sanatana.Notifications.SignalProviders.Interfaces;
using Sanatana.Notifications.SignalProviders.WCF;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.SignalProviders.WCF.DI.Autofac
{
    /// <summary>
    /// Register WcfSignalProvider to receive incoming events and dispatches. 
    /// Is optional. More than one SignalProvider can be registered.
    /// </summary>
    public class WcfSignalEndpointAutofacModule<TKey> : Module
        where TKey : struct
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WcfSignalProvider<TKey>>().As<ISignalProviderControl>().SingleInstance();
            builder.RegisterType<SignalServiceInstanceProvider<TKey>>().AsSelf().SingleInstance();
        }
    }
}

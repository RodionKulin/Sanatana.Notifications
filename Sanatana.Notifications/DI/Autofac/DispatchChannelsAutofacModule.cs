using Autofac;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Queries;
using Sanatana.Notifications.DispatchHandling.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DI.Autofac
{
    /// <summary>
    /// Register DispatchChannels including a sender and it's settings.
    /// </summary>
    public class DispatchChannelsAutofacModule<TKey> : Module
        where TKey : struct
    {
        private IEnumerable<DispatchChannel<TKey>> _dispatchChannels;


        public DispatchChannelsAutofacModule(IEnumerable<DispatchChannel<TKey>> dispatchChannels)
        {
            _dispatchChannels = dispatchChannels;
        }


        protected override void Load(ContainerBuilder builder)
        {
            foreach (DispatchChannel<TKey> item in _dispatchChannels)
            {
                builder.RegisterInstance(item).AsSelf().SingleInstance();
            }
        }
    }
}

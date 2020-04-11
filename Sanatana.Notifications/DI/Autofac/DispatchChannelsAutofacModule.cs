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
    /// Register DispatchChannels including IDispatcher.
    /// </summary>
    public class DispatchChannelsAutofacModule<TKey> : Module
        where TKey : struct
    {
        //fields
        private IEnumerable<DispatchChannel<TKey>> _dispatchChannels;


        //ctor
        public DispatchChannelsAutofacModule(IEnumerable<DispatchChannel<TKey>> dispatchChannels)
        {
            _dispatchChannels = dispatchChannels;
        }


        //methods
        protected override void Load(ContainerBuilder builder)
        {
            foreach (DispatchChannel<TKey> item in _dispatchChannels)
            {
                builder.RegisterInstance(item).AsSelf().SingleInstance();
            }
        }
    }
}

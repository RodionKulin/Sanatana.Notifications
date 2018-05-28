using Autofac;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DI.Autofac
{
    /// <summary>
    /// Register InMemoryEventSettingsQueries. 
    /// Single EventSettingsQueries implementation is required. Can be replaced by database EventSettingsQueries.
    /// </summary>
    public class InMemoryEventSettingsAutofacModule<TKey> : Module
        where TKey : struct
    {
        private IEnumerable<EventSettings<TKey>> _settings;


        public InMemoryEventSettingsAutofacModule(IEnumerable<EventSettings<TKey>> settings)
        {
            _settings = settings;
        }


        protected override void Load(ContainerBuilder builder)
        {
            var queries = new InMemoryEventSettingsQueries<TKey>(_settings);
            builder.RegisterInstance(queries).As<IEventSettingsQueries<TKey>>().SingleInstance();
        }
    }
}

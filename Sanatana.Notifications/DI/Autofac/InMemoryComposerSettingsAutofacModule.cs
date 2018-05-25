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
    /// Register InMemoryComposerSettingsQueries. 
    /// Single ComposerSettingsQueries implementation is required. Can be replaced by database ComposerSettingsQueries.
    /// </summary>
    public class InMemoryComposerSettingsAutofacModule<TKey> : Module
        where TKey : struct
    {
        private IEnumerable<ComposerSettings<TKey>> _settings;


        public InMemoryComposerSettingsAutofacModule(IEnumerable<ComposerSettings<TKey>> settings)
        {
            _settings = settings;
        }


        protected override void Load(ContainerBuilder builder)
        {
            var queries = new InMemoryComposerSettingsQueries<TKey>(_settings);
            builder.RegisterInstance(queries).As<IComposerSettingsQueries<TKey>>().SingleInstance();
        }
    }
}

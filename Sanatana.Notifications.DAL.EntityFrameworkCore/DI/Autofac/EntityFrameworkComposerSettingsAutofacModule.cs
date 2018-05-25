using Autofac;
using Sanatana.Notifications.DAL.EntityFrameworkCore;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.DI.Autofac
{
    /// <summary>
    /// Register EntityFrameworkCore ComposerSettingsQueries implementation that fetches settings from database.
    /// Single ComposerSettingsQueries implementation is required. Can be replaced by InMemoryComposerSettingsQueries.
    /// </summary>
    public class EntityFrameworkComposerSettingsAutofacModule : Module
    {
        bool _useCaching;

        /// <summary>
        /// </summary>
        /// <param name="useCaching">Enable caching of DispatchTemplate and ComposerSettings.</param>
        public EntityFrameworkComposerSettingsAutofacModule(bool useCaching)
        {
            _useCaching = useCaching;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (_useCaching)
            {
                builder.RegisterType<SqlComposerSettingsQueries>()
                    .Named<IComposerSettingsQueries<long>>("default")
                    .SingleInstance();
                builder.RegisterGenericDecorator(
                   typeof(CachedComposerSettingsQueries<>), typeof(IComposerSettingsQueries<>), fromKey: "default")
                    .SingleInstance();

                builder.RegisterType<SqlDispatchTemplateQueries>()
                    .Named<IDispatchTemplateQueries<long>>("default")
                    .SingleInstance();
                builder.RegisterGenericDecorator(
                   typeof(CachedDispatchTemplateQueries<>), typeof(IDispatchTemplateQueries<>), fromKey: "default")
                    .SingleInstance();
            }
            else
            {
                builder.RegisterType<SqlComposerSettingsQueries>().As<IComposerSettingsQueries<long>>().SingleInstance();
                builder.RegisterType<SqlDispatchTemplateQueries>().As<IDispatchTemplateQueries<long>>().SingleInstance();
            }
        }
    }
}

using Autofac;
using Sanatana.Notifications.DAL.EntityFrameworkCore;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Queries;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.DI.Autofac
{
    /// <summary>
    /// Register EntityFrameworkCore EventSettings and DispatchTemplates questies implementation that fetches settings from database.
    /// Single EventSettingsQueries implementation is required. Can be replaced by InMemoryEventSettingsQueries.
    /// </summary>
    public class EntityFrameworkEventSettingsAutofacModule : Module
    {
        bool _useCaching;

        /// <summary>
        /// </summary>
        /// <param name="useCaching">Enable caching of DispatchTemplate and EventSettings.</param>
        public EntityFrameworkEventSettingsAutofacModule(bool useCaching)
        {
            _useCaching = useCaching;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (_useCaching)
            {
                builder.RegisterType<SqlEventSettingsQueries>()
                    .Named<IEventSettingsQueries<long>>("default")
                    .SingleInstance();
                builder.RegisterGenericDecorator(
                   typeof(CachedEventSettingsQueries<>), typeof(IEventSettingsQueries<>), fromKey: "default")
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
                builder.RegisterType<SqlEventSettingsQueries>().As<IEventSettingsQueries<long>>().SingleInstance();
                builder.RegisterType<SqlDispatchTemplateQueries>().As<IDispatchTemplateQueries<long>>().SingleInstance();
            }
        }
    }
}

using Autofac;
using MongoDB.Bson;
using Sanatana.MongoDb;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.MongoDb.Queries;
using Sanatana.Notifications.DAL.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDb.DI.Autofac
{
    public class MongoDbEventSettingsAutofacModule : Module
    {
        bool _useCaching;


        public MongoDbEventSettingsAutofacModule(bool useCaching)
        {
            _useCaching = useCaching;
        }


        protected override void Load(ContainerBuilder builder)
        {

            if (_useCaching)
            {
                builder.RegisterType<MongoDbEventSettingsQueries>()
                    .Named<IEventSettingsQueries<long>>("default")
                    .SingleInstance();
                builder.RegisterGenericDecorator(
                   typeof(CachedEventSettingsQueries<>), typeof(IEventSettingsQueries<>), fromKey: "default")
                    .SingleInstance();

                builder.RegisterType<MongoDbDispatchTemplateQueries>()
                    .Named<IDispatchTemplateQueries<long>>("default")
                    .SingleInstance();
                builder.RegisterGenericDecorator(
                   typeof(CachedDispatchTemplateQueries<>), typeof(IDispatchTemplateQueries<>), fromKey: "default")
                    .SingleInstance();
            }
            else
            {
                builder.RegisterType<MongoDbEventSettingsQueries>()
                    .As<IEventSettingsQueries<ObjectId>>().SingleInstance();
                builder.RegisterType<MongoDbDispatchTemplateQueries>()
                    .As<IDispatchTemplateQueries<ObjectId>>().SingleInstance();
            }
        }
    }
}

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Sanatana.Notifications.Composing;
using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper
{
    public class NotificationsMapperFactory : INotificationsMapperFactory
    {
        //fields
        protected Lazy<IMapper> _mapper;


        //init
        public NotificationsMapperFactory()
        {
            _mapper = new Lazy<IMapper>(Create);
        }


        //methods
        public virtual IMapper GetMapper()
        {
            return _mapper.Value;
        }

        protected virtual IMapper Create()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                Configure(cfg);
            });

            return configuration.CreateMapper();
        }

        protected virtual void Configure(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<SignalBounce<long>, SignalBounceLong>();
            configuration.CreateMap<SignalEvent<long>, SignalEventLong>();
            configuration.CreateMap<StoredNotification<long>, StoredNotificationLong>();
            configuration.CreateMap<SubscriberDeliveryTypeSettings<long>, SubscriberDeliveryTypeSettingsLong>();
            configuration.CreateMap<SubscriberCategorySettings<long>, SubscriberCategorySettingsLong>();
            configuration.CreateMap<SubscriberTopicSettings<long>, SubscriberTopicSettingsLong>();
            configuration.CreateMap<SubscriberScheduleSettings<long>, SubscriberScheduleSettingsLong>();
            configuration.CreateMap<EventSettings<long>, EventSettingsLong>();

            configuration.CreateMap<SignalBounceLong, SignalBounce<long>>();
            configuration.CreateMap<SignalEventLong, SignalEvent<long>>();
            configuration.CreateMap<StoredNotificationLong, StoredNotification<long>>();
            configuration.CreateMap<SubscriberDeliveryTypeSettingsLong, SubscriberDeliveryTypeSettings<long>>();
            configuration.CreateMap<SubscriberCategorySettingsLong, SubscriberCategorySettings<long>>();
            configuration.CreateMap<SubscriberTopicSettingsLong, SubscriberTopicSettings<long>>();
            configuration.CreateMap<SubscriberScheduleSettingsLong, SubscriberScheduleSettings<long>>();
            configuration.CreateMap<EventSettingsLong, EventSettings<long>>();

            //Dispatch template
            configuration.CreateMap<DispatchTemplate<long>, DispatchTemplateLong>()
                .ForMember(d => d.DerivedEntityData, o => o.MapFrom<ToJsonValueResolver<DispatchTemplate<long>, DispatchTemplateLong>>());
            configuration.CreateMap<DispatchTemplateLong, DispatchTemplate<long>>()
                .ConstructUsing((serialized, generic) =>
                {
                    DispatchTemplate<long> derivedInstance = (DispatchTemplate<long>)JsonConvert.DeserializeObject(
                        serialized.DerivedEntityData, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Objects
                        });
                    return derivedInstance;
                });

            //Dispatch
            configuration.CreateMap<SignalDispatch<long>, SignalDispatchLong>()
                .ForMember(d => d.DerivedEntityData, o => o.MapFrom<ToJsonValueResolver<SignalDispatch<long>, SignalDispatchLong>>());
            configuration.CreateMap<SignalDispatchLong, SignalDispatch<long>>()
                .ConstructUsing((serialized, generic) =>
                {
                    SignalDispatch<long> derivedInstance = (SignalDispatch<long>)JsonConvert.DeserializeObject(
                        serialized.DerivedEntityData, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Objects
                        });
                    return derivedInstance;
                });

        }
    }
}

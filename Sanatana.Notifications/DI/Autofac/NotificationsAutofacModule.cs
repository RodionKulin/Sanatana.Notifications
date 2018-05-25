using Autofac;
using Sanatana.Notifications;
using Sanatana.Notifications.Composing;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Queries;
using Sanatana.Notifications.DeliveryTypes.StoredNotification;
using Sanatana.Notifications.Dispatching.Channels;
using Sanatana.Notifications.Flushing;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.Sender;
using Sanatana.Notifications.SignalProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DI.Autofac
{
    /// <summary>
    /// Register core Sanatana.Notifications components. 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class NotificationsAutofacModule<TKey> : Module
        where TKey : struct
    {
        protected SenderSettings _senderSettings;

        public NotificationsAutofacModule(SenderSettings senderSettings = null)
        {
            _senderSettings = senderSettings ?? new SenderSettings();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_senderSettings).AsSelf().SingleInstance();
            builder.RegisterType<ConsoleMonitor<TKey>>().As<IMonitor<TKey>>().SingleInstance();

            builder.RegisterType<SenderState<TKey>>().SingleInstance();
            builder.RegisterType<Sender<TKey>>().As<ISender>().SingleInstance();

            builder.RegisterType<DirectSignalProvider<TKey>>().As<IDirectSignalProvider<TKey>>().As<ISignalProviderControl>().SingleInstance();
            builder.RegisterType<DatabaseDispatchProvider<TKey>>().As<IRegularJob>().SingleInstance();
            builder.RegisterType<DatabaseEventProvider<TKey>>().As<IRegularJob>().SingleInstance();

            builder.RegisterType<EventQueue<TKey>>().As<IEventQueue<TKey>>().As<IRegularJob>().SingleInstance();
            builder.RegisterType<DispatchQueue<TKey>>().As<IDispatchQueue<TKey>>().As<IRegularJob>().SingleInstance();            
            builder.RegisterType<SignalEventFlushJob<TKey>>().As<ISignalFlushJob<SignalEvent<TKey>>>().As<IRegularJob>().SingleInstance();
            builder.RegisterType<SignalDispatchFlushJob<TKey>>().As<ISignalFlushJob<SignalDispatch<TKey>>>().As<IRegularJob>().SingleInstance();
            builder.RegisterType<StoredNotificationFlushJob<TKey>>().As<IStoredNotificationFlushJob<TKey>>().As<IRegularJob>().SingleInstance();

            builder.RegisterType<TemporaryStorage<SignalEvent<TKey>>>().As<ITemporaryStorage<SignalEvent<TKey>>>().SingleInstance();
            builder.RegisterType<TemporaryStorage<SignalDispatch<TKey>>>().As<ITemporaryStorage<SignalDispatch<TKey>>>().SingleInstance();

            builder.RegisterType<CompositionProcessor<TKey>>().As<IRegularJob>().SingleInstance();
            builder.RegisterType<DispatchingProcessor<TKey>>().As<IRegularJob>().SingleInstance();

            builder.RegisterType<CompositionHandlerRegistry<TKey>>().As<ICompositionHandlerRegistry<TKey>>().SingleInstance();
            builder.RegisterType<CompositionHandler<TKey>>().As<ICompositionHandler<TKey>>().SingleInstance();
            builder.RegisterType<DispatchBuilder<TKey>>().As<IDispatchBuilder<TKey>>().SingleInstance();
            builder.RegisterType<SubscribersFetcher<TKey>>().As<ISubscribersFetcher<TKey>>().SingleInstance();
            builder.RegisterType<ReceivePeriodScheduler<TKey>>().As<IScheduler<TKey>>().SingleInstance();

            builder.RegisterType<DispatchChannelRegistry<TKey>>().As<IDispatchChannelRegistry<TKey>>().SingleInstance();
            builder.RegisterType<StoredNotificationDispatcher<TKey>>().As<IStoredNotificationDispatcher<TKey>>().SingleInstance();
        }
    }
}

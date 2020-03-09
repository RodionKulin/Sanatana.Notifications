using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sanatana.Notifications.Composing;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Queries;
using Sanatana.Notifications.DeliveryTypes.StoredNotification;
using Sanatana.Notifications.Dispatching.Channels;
using Sanatana.Notifications.Flushing;
using Sanatana.Notifications.EventTracking;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Processing.Interfaces;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.Sender;
using Sanatana.Notifications.SignalProviders;
using Sanatana.Notifications.SignalProviders.Interfaces;

namespace Sanatana.Notifications.DI.Autofac
{
    /// <summary>
    /// Register core Sanatana.Notifications components. 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class NotificationsCoreAutofacModule<TKey> : Module
        where TKey : struct
    {
        protected SenderSettings _senderSettings;

        public NotificationsCoreAutofacModule(SenderSettings senderSettings = null)
        {
            _senderSettings = senderSettings ?? new SenderSettings();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_senderSettings).AsSelf().SingleInstance();
            builder.RegisterType<ConsoleEventTracker<TKey>>().As<IEventTracker<TKey>>().SingleInstance();

            builder.RegisterInstance(NullLogger.Instance)
                .IfNotRegistered(typeof(ILogger))
                .As<ILogger>()
                .SingleInstance();

            builder.RegisterType<SenderState<TKey>>().SingleInstance();
            builder.RegisterType<Sender<TKey>>().As<ISender>().SingleInstance();

            builder.RegisterType<DirectSignalProvider<TKey>>()
                .As<ISignalProvider<TKey>>()
                .As<ISignalProviderControl>()
                .SingleInstance();
            builder.RegisterType<DatabaseDispatchProvider<TKey>>()
                .As<IDatabaseDispatchProvider>()
                .As<IRegularJob>()
                .IfNotRegistered(typeof(IDatabaseDispatchProvider))
                .SingleInstance();
            builder.RegisterType<DatabaseEventProvider<TKey>>()
                .As<IDatabaseEventProvider>()
                .As<IRegularJob>()
                .IfNotRegistered(typeof(IDatabaseEventProvider))
                .SingleInstance();

            builder.RegisterType<EventQueue<TKey>>()
                .As<IEventQueue<TKey>>()
                .As<IRegularJob>()
                .IfNotRegistered(typeof(IEventQueue<TKey>))
                .SingleInstance();
            builder.RegisterType<DispatchQueue<TKey>>()
                .As<IDispatchQueue<TKey>>()
                .As<IRegularJob>()
                .IfNotRegistered(typeof(IDispatchQueue<TKey>))
                .SingleInstance();
            builder.RegisterType<SignalEventFlushJob<TKey>>()
                .As<ISignalFlushJob<SignalEvent<TKey>>>()
                .As<IRegularJob>()
                .IfNotRegistered(typeof(ISignalFlushJob<SignalEvent<TKey>>))
                .SingleInstance();
            builder.RegisterType<SignalDispatchFlushJob<TKey>>()
                .As<ISignalFlushJob<SignalDispatch<TKey>>>()
                .As<IRegularJob>()
                .IfNotRegistered(typeof(ISignalFlushJob<SignalDispatch<TKey>>))
                .SingleInstance();
            builder.RegisterType<StoredNotificationFlushJob<TKey>>()
                .As<IStoredNotificationFlushJob<TKey>>()
                .As<IRegularJob>()
                .IfNotRegistered(typeof(IStoredNotificationFlushJob<TKey>))
                .SingleInstance();

            builder.RegisterType<CompositionProcessor<TKey>>()
                .As<ICompositionProcessor>()
                .As<IRegularJob>()
                .IfNotRegistered(typeof(ICompositionProcessor))
                .SingleInstance();
            builder.RegisterType<DispatchingProcessor<TKey>>()
                .As<IDispatchingProcessor>()
                .As<IRegularJob>()
                .IfNotRegistered(typeof(IDispatchingProcessor))
                .SingleInstance();

            builder.RegisterType<FileRepository>().As<IFileRepository>().SingleInstance();
            builder.RegisterType<TemporaryStorage<SignalEvent<TKey>>>().As<ITemporaryStorage<SignalEvent<TKey>>>().SingleInstance();
            builder.RegisterType<TemporaryStorage<SignalDispatch<TKey>>>().As<ITemporaryStorage<SignalDispatch<TKey>>>().SingleInstance();

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

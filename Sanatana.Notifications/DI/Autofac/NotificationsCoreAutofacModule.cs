using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sanatana.Notifications.EventsHandling;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Queries;
using Sanatana.Notifications.DeliveryTypes.StoredNotification;
using Sanatana.Notifications.DispatchHandling.Channels;
using Sanatana.Notifications.Flushing;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.Sender;
using Sanatana.Notifications.SignalProviders;
using Sanatana.Notifications.SignalProviders.Interfaces;
using Sanatana.Notifications.Processing.DispatchProcessingCommands;
using Sanatana.Notifications.Flushing.Queues;
using Sanatana.Notifications.Locking;

namespace Sanatana.Notifications.DI.Autofac
{
    /// <summary>
    /// Register core Sanatana.Notifications components. 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class NotificationsCoreAutofacModule<TKey> : Module
        where TKey : struct
    {
        //fields
        protected SenderSettings _senderSettings;


        //ctor
        public NotificationsCoreAutofacModule(SenderSettings senderSettings = null)
        {
            _senderSettings = senderSettings ?? new SenderSettings();
        }


        //methods
        protected override void Load(ContainerBuilder builder)
        {
            RegisterSender(builder);
            RegisterLogging(builder);
            RegisterSignalProviders(builder);
            RegisterEvenHandling(builder);
            RegisterQueues(builder);
            RegisterStoredNotifications(builder);
            RegisterProcessors(builder);
            RegisterTempStorage(builder);
            RegisterDispatching(builder);
            RegisterLockTracker(builder);
        }

        protected virtual void RegisterSender(ContainerBuilder builder)
        {
            builder.RegisterInstance(_senderSettings).AsSelf().SingleInstance();
            builder.RegisterType<SenderState<TKey>>().SingleInstance();
            builder.RegisterType<Sender<TKey>>().As<ISender>().SingleInstance();
        }

        protected virtual void RegisterDispatching(ContainerBuilder builder)
        {
            builder.RegisterType<DispatchChannelRegistry<TKey>>().As<IDispatchChannelRegistry<TKey>>().SingleInstance();
        }

        protected virtual void RegisterLogging(ContainerBuilder builder)
        {
            builder.RegisterType<ConsoleMonitor<TKey>>().As<IMonitor<TKey>>().SingleInstance();
            builder.RegisterInstance(NullLogger.Instance)
                .IfNotRegistered(typeof(ILogger))
                .As<ILogger>()
                .SingleInstance();
        }

        protected virtual void RegisterEvenHandling(ContainerBuilder builder)
        {
            builder.RegisterType<EventHandlerRegistry<TKey>>().As<IEventHandlerRegistry<TKey>>().SingleInstance();
            builder.RegisterType<DefaultEventHandler<TKey>>().As<IEventHandler<TKey>>().SingleInstance();
            builder.RegisterType<DispatchBuilder<TKey>>().As<IDispatchBuilder<TKey>>().SingleInstance();
            builder.RegisterType<SubscribersFetcher<TKey>>().As<ISubscribersFetcher<TKey>>().SingleInstance();
            builder.RegisterType<ReceivePeriodScheduler<TKey>>().As<IScheduler<TKey>>().SingleInstance();
        }

        protected virtual void RegisterSignalProviders(ContainerBuilder builder)
        {
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
        }

        protected virtual void RegisterQueues(ContainerBuilder builder)
        {
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
        }

        protected virtual void RegisterProcessors(ContainerBuilder builder)
        {
            builder.RegisterType<DispatchProcessor<TKey>>()
                .As<IDispatchProcessor>()
                .As<IRegularJob>()
                .IfNotRegistered(typeof(IDispatchProcessor))
                .SingleInstance();

            builder.RegisterType<ConsolidateDispatchCommand<TKey>>().As<IDispatchProcessingCommand<TKey>>();
            builder.RegisterType<SendDispatchCommand<TKey>>().As<IDispatchProcessingCommand<TKey>>();
            builder.RegisterType<InsertDispatchHistoryCommand<TKey>>()
                .As<IDispatchProcessingCommand<TKey>>()
                .As<IRegularJob>();

            builder.RegisterType<EventProcessor<TKey>>()
                .As<IEventProcessor>()
                .As<IRegularJob>()
                .IfNotRegistered(typeof(IEventProcessor))
                .SingleInstance();
        }

        protected virtual void RegisterTempStorage(ContainerBuilder builder)
        {
            builder.RegisterType<FileRepository>().As<IFileRepository>().SingleInstance();
            builder.RegisterType<TemporaryStorage<SignalEvent<TKey>>>().As<ITemporaryStorage<SignalEvent<TKey>>>().SingleInstance();
            builder.RegisterType<TemporaryStorage<SignalDispatch<TKey>>>().As<ITemporaryStorage<SignalDispatch<TKey>>>().SingleInstance();


        }

        protected virtual void RegisterStoredNotifications(ContainerBuilder builder)
        {
            builder.RegisterType<StoredNotificationDispatcher<TKey>>().As<IStoredNotificationDispatcher<TKey>>().SingleInstance();
            builder.RegisterType<StoredNotificationFlushJob<TKey>>()
                .As<IStoredNotificationFlushJob<TKey>>()
                .As<IRegularJob>()
                .IfNotRegistered(typeof(IStoredNotificationFlushJob<TKey>))
                .SingleInstance();
        }

        protected virtual void RegisterLockTracker(ContainerBuilder builder)
        {
            builder.RegisterType<LockTracker<TKey>>().As<ILockTracker<TKey>>().SingleInstance();
        }
    }
}

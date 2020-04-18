using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.Flushing;
using Sanatana.Notifications.Models;
using Sanatana.Notifications.Sender;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Processing.DispatchProcessingCommands
{
    public class InsertDispatchHistoryCommand<TKey> : FlushJobBase<SignalDispatch<TKey>>, IDispatchProcessingCommand<TKey>
        where TKey : struct
    {
        //fields
        protected IEventSettingsQueries<TKey> _eventSettingsQueries;


        //properties
        public int Order { get; set; } = 3;


        //ctor
        public InsertDispatchHistoryCommand(SenderSettings senderSettings, ISignalDispatchHistoryQueries<TKey> historyQueries,
            IEventSettingsQueries<TKey> eventSettingsQueries)
            : base(senderSettings)
        {
            _eventSettingsQueries = eventSettingsQueries;
            _flushQueues[FlushAction.Insert] = new FlushQueue<SignalDispatch<TKey>>(items => historyQueries.InsertMany(items));
        }


        //methods
        public virtual bool Execute(SignalWrapper<SignalDispatch<TKey>> item)
        {
            EventSettings<TKey> eventSettings = item.Signal.EventSettingsId == null
                ? null
                : _eventSettingsQueries.Select(item.Signal.EventSettingsId.Value).Result;
            if (eventSettings == null)
            {
                return true;
            }

            if (eventSettings.StoreInHistory)
            {
                _flushQueues[FlushAction.Insert].Queue.Add(item.Signal);
            }

            return true;
        }
    }
}

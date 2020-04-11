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
        //properties
        public int Order { get; set; } = 2;


        //ctor
        public InsertDispatchHistoryCommand(SenderSettings senderSettings, ISignalDispatchHistoryQueries<TKey> queries)
            : base(senderSettings)
        {
            _flushQueues[FlushAction.Insert] = new FlushQueue<SignalDispatch<TKey>>(items => queries.InsertMany(items));
        }


        //methods
        public virtual Task<bool> Execute(SignalWrapper<SignalDispatch<TKey>> item)
        {
            if (item.Signal.StoreInHistory)
            {
                _flushQueues[FlushAction.Insert].Queue.Add(item.Signal);
            }

            return Task.FromResult(true);
        }
    }
}

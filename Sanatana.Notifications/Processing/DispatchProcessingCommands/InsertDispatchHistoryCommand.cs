using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Processing.DispatchProcessingCommands
{
    public class InsertDispatchHistoryCommand<TKey> : IDispatchProcessingCommand<TKey>
        where TKey : struct
    {
        //fields
        protected ISignalDispatchHistoryQueries<TKey> _signalDispatchHistoryQueries;


        //properties
        public int Order { get; set; }


        //ctor
        public InsertDispatchHistoryCommand(ISignalDispatchHistoryQueries<TKey> signalDispatchHistoryQueries)
        {
            _signalDispatchHistoryQueries = signalDispatchHistoryQueries;
        }


        //methods
        public virtual async Task<bool> Execute(SignalWrapper<SignalDispatch<TKey>> item)
        {
            if (!item.Signal.StoreInHistory)
            {
                return true;
            }

            await _signalDispatchHistoryQueries
                .Insert(new List<SignalDispatch<TKey>> { item.Signal })
                .ConfigureAwait(false);
            return true;
        }
    }
}

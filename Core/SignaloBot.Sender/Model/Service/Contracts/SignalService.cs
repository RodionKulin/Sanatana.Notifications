using Common.Utility;
using SignaloBot.DAL;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SignalService<TKey> : ISignalService<TKey>
        where TKey : struct
    {
        //поля
        private IEventQueue<TKey> _queue;
        private IStatisticsCollector<TKey> _stats;



        //инициализация
        public SignalService(IEventQueue<TKey> queue, IStatisticsCollector<TKey> stats)
        {
            _queue = queue;
            _stats = stats;
        }


        //методы
        public Task RaiseKeyValueEvent(TKey? groupID, TKey? userID
            , int categoryID, string topicID, Dictionary<string, string> values)
        {
            var keyValueEvent = new KeyValueEvent<TKey>()
            {
                ReceiveDateUtc = DateTime.UtcNow,
                CategoryID = categoryID,
                TopicID = topicID,
                Values = values,
                GroupID = groupID,
                UserIDRangeFrom = userID,
                UserIDRangeTo = userID
            };
            
            var signalWrapper = new SignalWrapper<SignalEventBase<TKey>>(keyValueEvent, false);
            _queue.Append(signalWrapper);

            if(_stats != null)
            {
                _stats.EventTransferred(keyValueEvent);
            }

            return Task.FromResult(0);
        }
                        
    }
}

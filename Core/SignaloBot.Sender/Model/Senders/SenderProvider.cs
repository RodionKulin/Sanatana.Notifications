using SignaloBot.DAL.Entities.Core;
using SignaloBot.Sender.Senders.FailCounter;
using SignaloBot.Sender.Senders.LimitManager;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Senders
{
    public class SenderProvider<T> : IDisposable
        where T : IMessage
    {
        //поля
        protected Dictionary<int, SendChannel<T>> _senders;


        //инициализация
        public SenderProvider()
        {
            _senders = new Dictionary<int, SendChannel<T>>();
        }


        //методы
        public virtual void Register(int deliveryType, ISender<T> sender, ILimitManager limitManager = null)
        {
            if (_senders.ContainsKey(deliveryType))
            {
                var exception = new Exception("Ключ " + deliveryType + " уже зарегистриован");

                throw exception;
            }

            if (limitManager == null)
                limitManager = new NoLimitManager();

            var sendChannel = new SendChannel<T>()
            {
                DeliveryType = deliveryType,
                Sender = sender,
                LimitManager = limitManager,
                FailCounter = new ProgressiveFailCounter()
            };
            _senders.Add(deliveryType, sendChannel);
        }

        public virtual SendChannel<T> MatchSender(T message)
        {
            return _senders[message.DeliveryType];
        }

        internal virtual List<SendChannel<T>> GetSenders()
        {
            return _senders.Values.ToList();
        }

        internal virtual List<SendChannel<T>> GetActiveSenders()
        {
            return _senders.Where(p => p.Value.IsActive)
                .Select(p => p.Value)
                .ToList();
        }

        internal virtual List<int> GetActiveSenderTypes()
        {
            return GetActiveSenders().Select(p => p.DeliveryType).ToList();
        }


        //IDisposable
        public virtual void Dispose()
        {
            foreach (SendChannel<T> sender in _senders.Values)
            {
                sender.Dispose();
            }
        }
    }
}

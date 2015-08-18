using SignaloBot.DAL;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Queries.Sender
{
    public interface IQueueQueries<T> : IDisposable
        where T : IMessage
    {
        List<T> Select(int count, List<int> deliveryTypes, int maxFailedAttempts);

        void Update(T message);

        void Delete(T message);
    }
}

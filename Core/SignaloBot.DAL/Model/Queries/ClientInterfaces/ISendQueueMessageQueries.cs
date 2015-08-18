using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Entities.Parameters;
using System;
using System.Collections.Generic;

namespace SignaloBot.DAL.Queries.Client
{
    public interface ISignalQueries
    {
        void Insert(List<Signal> messages, out Exception exception);
        List<Signal> SelectDelayed(Guid userID, int deliveryType, int categoryID, out Exception exception);
        List<Signal> SelectDelayed(Guid userID, int deliveryType, out Exception exception);
        void UpdateSendDateUtc(List<Signal> messages, out Exception exception);
        void UpdateCounters(UpdateParameters messageParameters
            , List<Signal> messages, out Exception exception);
    }
}

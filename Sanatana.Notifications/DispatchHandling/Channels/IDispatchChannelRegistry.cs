using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using System.Collections.Generic;

namespace Sanatana.Notifications.DispatchHandling.Channels
{
    public interface IDispatchChannelRegistry<TKey>
        where TKey : struct
    {
        List<int> GetActiveDeliveryTypes(bool checkLimitCapacity);
        DispatchChannel<TKey> Match(SignalDispatch<TKey> signal);
        List<DispatchChannel<TKey>> GetAll();
    }
}
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DispatchHandling;
using Sanatana.Notifications.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sanatana.Notifications.DispatchHandling.Channels
{
    public class DispatchChannelRegistry<TKey> : IDispatchChannelRegistry<TKey>
        where TKey : struct
    {
        //fields
        protected List<DispatchChannel<TKey>> _channels;
        protected ILogger _logger;


        //init
        public DispatchChannelRegistry(ILogger logger, IEnumerable<DispatchChannel<TKey>> channels)
        {
            _logger = logger;
            channels = channels ?? new List<DispatchChannel<TKey>>();
            _channels = channels.ToList();
        }


        //methods
        /// <summary>
        /// Get delivery types that have dispatchers with no penalty or limit reached.
        /// </summary>
        /// <param name="checkLimitCapacity">Limit capacity should only be check after if was set by Channel.SetLimitCapacity method.</param>
        /// <returns></returns>
        public virtual List<int> GetActiveDeliveryTypes(bool checkLimitCapacity)
        {
            IEnumerable<DispatchChannel<TKey>> activeChannels = _channels
                .Where(p => p.IsActive);

            if (checkLimitCapacity)
            {
                activeChannels = activeChannels.Where(p => p.AvailableLimitCapacity > 0);
            }

            return activeChannels
                .Select(p => p.DeliveryType)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Find DispatchChannel matching a signal.
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public virtual DispatchChannel<TKey> Match(SignalDispatch<TKey> signal)
        {
            DispatchChannel<TKey> dispatchChannel = _channels.FirstOrDefault(
                    p => p.IsActive
                    && p.AvailableLimitCapacity > 0
                    && p.DeliveryType == signal.DeliveryType);

            if (dispatchChannel == null)
            {
                _logger.LogError(SenderInternalMessages.DispatchChannelManager_NotFound,
                    typeof(DispatchChannel<TKey>), nameof(DispatchChannel<TKey>.DeliveryType), signal.DeliveryType);
            }

            return dispatchChannel;
        }

        /// <summary>
        /// Get all DispatchChannels
        /// </summary>
        /// <returns></returns>
        public virtual List<DispatchChannel<TKey>> GetAll()
        {
            return _channels;
        }
    }
}

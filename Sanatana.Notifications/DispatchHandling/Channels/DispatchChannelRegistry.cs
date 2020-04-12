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
        protected List<IDispatchChannel<TKey>> _channels;
        protected ILogger _logger;


        //init
        public DispatchChannelRegistry(ILogger logger, IEnumerable<IDispatchChannel<TKey>> channels)
        {
            _logger = logger;
            _channels = channels?.ToList() ?? new List<IDispatchChannel<TKey>>();
        }


        //methods
        /// <summary>
        /// Get delivery types that have dispatchers with no penalty or limit reached.
        /// </summary>
        /// <param name="checkLimitCapacity">Limit capacity should only be check after if was set by Channel.SetLimitCapacity method.</param>
        /// <returns></returns>
        public virtual List<int> GetActiveDeliveryTypes(bool checkLimitCapacity)
        {
            IEnumerable<IDispatchChannel<TKey>> activeChannels = _channels
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
        public virtual IDispatchChannel<TKey> Match(SignalDispatch<TKey> signal)
        {
            IDispatchChannel<TKey> channel = _channels.FirstOrDefault(
                p => p.DeliveryType == signal.DeliveryType);

            if (channel == null)
            {
                _logger.LogError(SenderInternalMessages.Common_NoServiceWithKeyFound,
                    typeof(IDispatchChannel<TKey>), nameof(IDispatchChannel<TKey>.DeliveryType), signal.DeliveryType);
            }

            if (!channel.IsActive || channel.AvailableLimitCapacity == 0)
            {
                _logger.LogError(SenderInternalMessages.DispatchChannelRegistry_InactiveChannelRequested,
                    signal.DeliveryType, channel.AvailableLimitCapacity);
                return null;
            }

            return channel;
        }

        /// <summary>
        /// Get all DispatchChannels
        /// </summary>
        /// <returns></returns>
        public virtual List<IDispatchChannel<TKey>> GetAll()
        {
            return _channels;
        }
    }
}

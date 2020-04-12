using Sanatana.Notifications.EventsHandling;
using Sanatana.Notifications.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sanatana.Notifications.EventsHandling
{
    public class EventHandlerRegistry<TKey> : IEventHandlerRegistry<TKey>
        where TKey : struct
    {
        //fields
        protected List<IEventHandler<TKey>> _eventHandlers;
        protected ILogger _logger;


        //init
        public EventHandlerRegistry(IEnumerable<IEventHandler<TKey>> eventHandlers, ILogger logger)
        {
            _eventHandlers = eventHandlers.ToList();
            _logger = logger;
        }


        //methods
        public virtual IEventHandler<TKey> MatchHandler(int? handlerId)
        {
            IEventHandler<TKey>[] matchingHandlers = _eventHandlers
                .Where(x => x.EventHandlerId == handlerId)
                .ToArray();

            if (matchingHandlers.Length == 0)
            {
                string error = string.Format(SenderInternalMessages.Common_NoServiceWithKeyFound,
                    typeof(IEventHandler<TKey>), nameof(IEventHandler<TKey>.EventHandlerId), handlerId);
                _logger.LogError(error);
            }
            else if (matchingHandlers.Length > 1)
            {
                string error = string.Format(SenderInternalMessages.Common_MoreThanOneServiceWithKeyFound,
                    typeof(IEventHandler<TKey>), nameof(IEventHandler<TKey>.EventHandlerId), handlerId);
                _logger.LogError(error);
            }

            return matchingHandlers.FirstOrDefault();
        }
    }
}

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
        protected IEnumerable<IEventHandler<TKey>> _eventHandlers;
        protected ILogger _logger;


        //init
        public EventHandlerRegistry(IEnumerable<IEventHandler<TKey>> compositionHandlers, ILogger logger)
        {
            _eventHandlers = compositionHandlers;
            _logger = logger;
        }


        //methods
        public virtual IEventHandler<TKey> MatchHandler(int? handlerId)
        {
            IEventHandler<TKey> handler = _eventHandlers.FirstOrDefault(
                x => x.EventHandlerId == handlerId);
            int handlerIdCount = _eventHandlers.Count(x => x.EventHandlerId == handlerId);

            if (handler == null)
            {
                string error = string.Format(SenderInternalMessages.CompositionHandlerFactory_NotFound,
                    typeof(IEventHandler<TKey>), nameof(IEventHandler<TKey>.EventHandlerId), handlerId);
                _logger.LogError(error);
            }
            else if (handlerIdCount > 1)
            {
                string error = string.Format(SenderInternalMessages.CompositionHandlerFactory_MoreThanOneFound, 
                    typeof(IEventHandler<TKey>), nameof(IEventHandler<TKey>.EventHandlerId), handlerId);
                _logger.LogError(error);
            }

            return handler;
        }
    }
}

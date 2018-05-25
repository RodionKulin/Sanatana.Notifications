using Sanatana.Notifications.Composing;
using Sanatana.Notifications.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sanatana.Notifications.Composing
{
    public class CompositionHandlerRegistry<TKey> : ICompositionHandlerRegistry<TKey>
        where TKey : struct
    {
        //fields
        protected IEnumerable<ICompositionHandler<TKey>> _compositionHandlers;
        protected ILogger _logger;


        //init
        public CompositionHandlerRegistry(IEnumerable<ICompositionHandler<TKey>> compositionHandlers,
            ILogger logger)
        {
            _compositionHandlers = compositionHandlers;
            _logger = logger;
        }


        //methods
        public virtual ICompositionHandler<TKey> MatchHandler(int? handlerId)
        {
            ICompositionHandler<TKey> handler = _compositionHandlers.FirstOrDefault(
                x => x.CompositionHandlerId == handlerId);
            int handlerIdCount = _compositionHandlers.Count(x => x.CompositionHandlerId == handlerId);

            if (handler == null)
            {
                string error = string.Format(SenderInternalMessages.CompositionHandlerFactory_NotFound,
                    typeof(ICompositionHandler<TKey>), nameof(ICompositionHandler<TKey>.CompositionHandlerId), handlerId);
                _logger.LogError(error);
            }
            else if (handlerIdCount > 1)
            {
                string error = string.Format(SenderInternalMessages.CompositionHandlerFactory_MoreThanOneFound, 
                    typeof(ICompositionHandler<TKey>), nameof(ICompositionHandler<TKey>.CompositionHandlerId), handlerId);
                _logger.LogError(error);
            }

            return handler;
        }
    }
}

using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sanatana.Notifications.Resources;
using Sanatana.Notifications.DispatchHandling.Consolidation;
using Newtonsoft.Json;
using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.Queues;

namespace Sanatana.Notifications.Processing.DispatchProcessingCommands
{
    public class ConsolidateDispatchCommand<TKey> : IDispatchProcessingCommand<TKey>
        where TKey: struct
    {
        //fields
        protected ISignalDispatchQueries<TKey> _signalDispatchQueries;
        protected ILogger _logger;
        protected ITemplateDataConsolidator[] _consolidators;
        protected IDispatchQueue<TKey> _dispatchQueue;
        protected IEventSettingsQueries<TKey> _eventSettingsQueries;


        //properties
        public int Order { get; set; } = 0;


        //ctor
        public ConsolidateDispatchCommand(ILogger logger, ISignalDispatchQueries<TKey> signalDispatchQueries,
            IDispatchQueue<TKey> dispatchQueue, IEnumerable<ITemplateDataConsolidator> consolidators,
            IEventSettingsQueries<TKey> eventSettingsQueries)
        {
            _logger = logger;
            _signalDispatchQueries = signalDispatchQueries;
            _dispatchQueue = dispatchQueue;
            _consolidators = consolidators.ToArray();
            _eventSettingsQueries = eventSettingsQueries;
        }


        //methods
        public virtual bool Execute(SignalWrapper<SignalDispatch<TKey>> item)
        {
            if (item.Signal.EventSettingsId == null)
            {
                //SignalDispatch can be provided by ISignalProver without EventSettingsId 
                return true;
            }

            EventSettings<TKey> eventSettings = _eventSettingsQueries.Select(item.Signal.EventSettingsId.Value).Result;
            if (eventSettings == null)
            {
                _logger.LogError(SenderInternalMessages.Common_NoServiceWithKeyFound,
                    typeof(EventSettings<TKey>), nameof(EventSettings<TKey>.EventSettingsId), item.Signal.EventSettingsId.Value);
                _dispatchQueue.ApplyResult(item, ProcessingResult.NoHandlerFound);
                return false;
            }

            bool shouldConsolidate = ValidateRequiredProperties(item.Signal, eventSettings);
            if (!shouldConsolidate)
            {
                //Continue processing dispatch without consolidation
                return true;
            }

            ITemplateDataConsolidator consolidator = MatchConsolidator(item.Signal, eventSettings.ConsolidatorId.Value);
            if(consolidator == null)
            {
                _dispatchQueue.ApplyResult(item, ProcessingResult.NoHandlerFound);
                return false;
            }

            DispatchTemplate<TKey> dispatchTemplate = eventSettings.Templates == null
                ? null
                : eventSettings.Templates.FirstOrDefault(x => EqualityComparer<TKey>.Default.Equals(x.DispatchTemplateId, item.Signal.DispatchTemplateId.Value));
            if (dispatchTemplate == null)
            {
                _logger.LogError(SenderInternalMessages.Common_NoServiceWithKeyFound,
                    typeof(DispatchTemplate<TKey>), nameof(DispatchTemplate<TKey>.DispatchTemplateId), item.Signal.DispatchTemplateId.Value);
                _dispatchQueue.ApplyResult(item, ProcessingResult.NoHandlerFound);
                return false;
            }

            IEnumerable<TemplateData[]> batches = GetTemplateDataBatches(item, consolidator.BatchSize);
            TemplateData consolidatedData = consolidator.Consolidate(batches);
            dispatchTemplate.Update(item.Signal, consolidatedData);
            item.IsUpdated = true;

            return true;
        }

        protected virtual bool ValidateRequiredProperties(SignalDispatch<TKey> signal, EventSettings<TKey> eventSettings)
        {
            if (eventSettings.ConsolidatorId == null)
            {
                return false;
            }

            if (signal.CategoryId == null)
            {
                _logger.LogError(SenderInternalMessages.ConsolidationDispatchCommand_MissingArguments,
                    nameof(signal.CategoryId), signal.SignalDispatchId);
                return false;
            }

            if (signal.ReceiverSubscriberId == null)
            {
                _logger.LogError(SenderInternalMessages.ConsolidationDispatchCommand_MissingArguments,
                    nameof(signal.ReceiverSubscriberId), signal.SignalDispatchId);
                return false;
            }

            if (signal.DispatchTemplateId == null)
            {
                _logger.LogError(SenderInternalMessages.ConsolidationDispatchCommand_MissingArguments,
                    nameof(signal.DispatchTemplateId), signal.SignalDispatchId);
                return false;
            }

            return true;
        }

        protected virtual ITemplateDataConsolidator MatchConsolidator(SignalDispatch<TKey> signal, int consolidatorId)
        {
            ITemplateDataConsolidator[] matched = _consolidators
              .Where(x => x.ConsolidatorId == consolidatorId)
              .ToArray();

            if (matched.Length == 0)
            {
                _logger.LogError(SenderInternalMessages.Common_NoServiceWithKeyFound,
                    typeof(ITemplateDataConsolidator), nameof(ITemplateDataConsolidator.ConsolidatorId), consolidatorId);
            }
            else if (matched.Length > 1)
            {
                _logger.LogError(SenderInternalMessages.Common_MoreThanOneServiceWithKeyFound,
                    typeof(ITemplateDataConsolidator), nameof(ITemplateDataConsolidator.ConsolidatorId), consolidatorId);
            }

            return matched.FirstOrDefault();
        }

        protected virtual IEnumerable<TemplateData[]> GetTemplateDataBatches(
            SignalWrapper<SignalDispatch<TKey>> item, int? batchSize)
        {
            if(item.ConsolidatedSignals != null && item.ConsolidatedSignals.Length > 0)
            {
                yield return item.ConsolidatedSignals
                    .Select(x => x.Signal)
                    .Select(DeserializeTemplateData)
                    .Where(x => x != null)
                    .ToArray();
            }

            batchSize = batchSize ?? 1000;
            DateTime? previousBatchLatest = null;

            var categories = new List<(int deliveryType, int category)>();
            categories.Add((item.Signal.DeliveryType, item.Signal.CategoryId.Value));

            while (true)
            {
                List<SignalDispatch<TKey>> sameCategoryDispatches = _signalDispatchQueries.SelectConsolidated(
                    pageSize: batchSize.Value,
                    subscriberIds: new List<TKey> { item.Signal.ReceiverSubscriberId.Value },
                    categories: categories,
                    createdBefore: item.Signal.SendDateUtc,
                    createdAfter: previousBatchLatest)
                    .Result;

                if (sameCategoryDispatches.Count < batchSize)
                {
                    break;
                }

                //assume already ordered by CreateDateUtc
                previousBatchLatest = sameCategoryDispatches.LastOrDefault()?.CreateDateUtc;

                yield return sameCategoryDispatches
                    .Select(DeserializeTemplateData)
                    .Where(x => x != null)
                    .ToArray();
            }
        }

        protected virtual TemplateData DeserializeTemplateData(SignalDispatch<TKey> dispatch)
        {
            object templateDataObj = null;
            try
            {
                templateDataObj = JsonConvert.DeserializeObject(dispatch.TemplateData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, SenderInternalMessages.Common_DeserializeError,
                    nameof(dispatch.TemplateData), dispatch.TemplateData,
                    nameof(SignalDispatch<TKey>), dispatch.SignalDispatchId);
                return null;
            }

            Dictionary<string, string> templateDataDict = null;
            if (templateDataObj is Dictionary<string, string>)
            {
                templateDataDict = (Dictionary<string, string>)templateDataObj;
                templateDataObj = null;
            }

            return new TemplateData(templateDataDict, templateDataObj);
        }
    }
}

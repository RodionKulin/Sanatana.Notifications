using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.EventsHandling.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DispatchHandling.Consolidation
{
    /// <summary>
    /// Handles combining multiple SignalDispatch's TemplateData into one.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface ITemplateDataConsolidator
    {
        //properties
        /// <summary>
        /// Identifier used to find ITemplateDataConsolidator matching SignalDispatch ConsolidatorId.
        /// </summary>
        int? ConsolidatorId { get; }
        /// <summary>
        /// Optional parameter. Number of SignalDispatch items selected from database in single batch.
        /// Default value is 1000.
        /// </summary>
        int? BatchSize { get; }


        //methods
        TemplateData Consolidate(IEnumerable<TemplateData[]> templateDataBatches);
    }
}

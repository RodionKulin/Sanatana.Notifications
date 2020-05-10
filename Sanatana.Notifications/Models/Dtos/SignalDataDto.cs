using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.Models
{
    public class SignalDataDto
    {
        /// <summary>
        /// EventKey leading to EventSettings describing how SignalEvent should be processed.
        /// </summary>
        public int EventKey { get; set; }
        /// <summary>
        /// Key values pairs to insert into dispatch template. 
        /// Only one property TemplateDataDict or TemplateDataObj is required depending on TemplateTransformer used. 
        /// Or both if multiple DispatchTemplates assigned to EventSettings with different TemplateTransformer.
        /// </summary>
        public Dictionary<string, string> TemplateDataDict { get; set; }
        /// <summary>
        /// Model object to insert into template.
        /// Only one property TemplateDataDict or TemplateDataObj is required depending on TemplateTransformer used. 
        /// Or both if multiple DispatchTemplates assigned to EventSettings with different TemplateTransformer.
        /// </summary>
        public string TemplateDataObj { get; set; }
        /// <summary>
        /// MachineName metadata where Sender API was triggered.
        /// For debuggin purpose that will be attached to SignalEvent and SignalDispatch.
        /// </summary>
        public string MachineName { get; set; }
        /// <summary>
        /// ApplicationName metadata where Sender API was triggered.
        /// For debuggin purpose that will be attached to SignalEvent and SignalDispatch.
        /// </summary>
        public string ApplicationName { get; set; }
    }
}

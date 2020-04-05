using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RazorEngineCore;

namespace Sanatana.Notifications.EventsHandling.Templates
{
    public class RazorTransformer : ITemplateTransformer
    {
        //fields
        protected TemplateCache _longTermTemplatesCache = new TemplateCache();


        //properties
        /// <summary>
        /// Create compiled razor templates only once and reuse on next calls. Default is true.
        /// Memory Cache is persistent if EventSettings are stored in memory.
        /// But will be wiped out on EventSettings reading if EventSettings are stored in database.
        /// </summary>
        public bool UseLongTermCaching { get; set; } = true;



        //methods
        public Dictionary<TemplateData, string> Transform(ITemplateProvider templateProvider, List<TemplateData> templateData)
        {
            if (templateProvider == null)
            {
                return new Dictionary<TemplateData, string>();
            }

            return templateData.ToDictionary(data => data, data =>
            {
                var template = UseLongTermCaching
                    ? (RazorEngineCompiledTemplate)_longTermTemplatesCache.GetOrCreate(data.Language, () => GetCompiledTemplate(templateProvider, data))
                    : GetCompiledTemplate(templateProvider, data);

                return data.ObjectModel == null
                    ? template.Run(data.KeyValueModel)
                    : template.Run(data.ObjectModel);
            });
        }

        protected virtual RazorEngineCompiledTemplate GetCompiledTemplate(ITemplateProvider templateProvider, TemplateData data)
        {
            string template = templateProvider.ProvideTemplate(data.Language);
            var razorEngine = new RazorEngine();
            return razorEngine.Compile(template);
        }

    }
}

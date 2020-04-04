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
        protected static TemplateCache _longTermTemplatesCache = new TemplateCache(null);


        //properties
        /// <summary>
        /// Create compiled razor templates only once and reuse on next calls.
        /// </summary>
        public bool UseLongTermCaching { get; set; } = true;



        //methods
        public Dictionary<TemplateData, string> Transform(ITemplateProvider templateProvider, List<TemplateData> templateData)
        {
            if (templateProvider == null)
            {
                return new Dictionary<TemplateData, string>();
            }

            TemplateCache templateCache = UseLongTermCaching
                ? _longTermTemplatesCache
                : new TemplateCache(templateProvider);

            return templateData.ToDictionary(data => data, data =>
            {
                RazorEngineCompiledTemplate template = GetCompiledTemplate(templateProvider, templateCache, data);
                return data.ObjectModel == null
                    ? template.Run(data.KeyValueModel)
                    : template.Run(data.ObjectModel);
            });
        }

        protected virtual RazorEngineCompiledTemplate GetCompiledTemplate(ITemplateProvider templateProvider, 
            TemplateCache templateCache, TemplateData data)
        {
            var transform = (RazorEngineCompiledTemplate)templateCache.GetItem(data.Culture);
            if (transform != null)
            {
                return transform;
            }

            string template = templateProvider.ProvideTemplate(data.Culture);

            RazorEngine razorEngine = new RazorEngine();
            transform = razorEngine.Compile(template);

            templateCache.InsertItem(transform, data.Culture);
            return transform;
        }

    }
}

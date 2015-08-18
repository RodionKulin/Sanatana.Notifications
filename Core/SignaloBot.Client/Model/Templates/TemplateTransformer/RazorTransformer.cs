using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignaloBot.Client.Templates
{
    public class RazorTransformer : ITemplateTransformer
    {
        bool _engineInitialized;


        //инициализация
        public RazorTransformer(string templatesRelativePath)
        {
            if (!_engineInitialized)
            {
                Engine.Razor = InitEngineService(templatesRelativePath);
                _engineInitialized = true;
            }
        }

        protected virtual IRazorEngineService InitEngineService(string templatesRelativePath)
        {
            var config = new TemplateServiceConfiguration()
            {
                Language = Language.CSharp,
                Debug = false,

                //trusted environment, no dynamic templates, debug disabled
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { }),

                TemplateManager = new RazorTemplateManager(templatesRelativePath)
            };

            IRazorEngineService service = RazorEngineService.Create(config);
            return service;
        }
        


        //методы
        public virtual string Transform(string templateName, Dictionary<string, string> replaceStrings)
        {
            var viewBag = new DynamicViewBag();
            foreach (var item in replaceStrings)
            {
                viewBag.AddValue(item.Key, item.Value);
            }

            return Engine.Razor.RunCompile(templateName, viewBag: viewBag);
        }

        public virtual string Transform(string templateName, object objectModel)
        {
            return Engine.Razor.RunCompile(templateName, null, objectModel);
        }
        
        public virtual List<string> TransformList(ITemplateProvider templateProvider, List<TemplateData> templateData)
        {
            CultureInfo threadCulture = Thread.CurrentThread.CurrentCulture;
            
            List<string> list = new List<string>();
            TemplateCache templateCache = new TemplateCache(templateProvider);

            foreach (TemplateData data in templateData)
            {
                string templateName = templateCache.ProvideTemplate(data.Variant, data.Culture);
                Thread.CurrentThread.CurrentCulture = data.Culture ?? threadCulture;

                string content = null;
                if (data.ObjectModel != null)
                {
                    content = Transform(templateName, data.ObjectModel);
                }
                else
                {
                    var replaceModel = data.ReplaceModel ?? new Dictionary<string, string>();
                    content = Transform(templateName, replaceModel);
                }

                list.Add(content);
            }
                        
            Thread.CurrentThread.CurrentCulture = threadCulture;

            return list;
        }
                
    }
}

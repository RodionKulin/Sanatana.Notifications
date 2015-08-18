using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SignaloBot.Client.Templates
{
    public class ReplaceTransformer : ITemplateTransformer
    {
        //свойства
        public string KeyFormat { get; set; }


        //инициализация
        public ReplaceTransformer(string keyFormat = null)
        {
            KeyFormat = keyFormat ?? "{{{0}}}";
        }
 

        //методы
        public virtual string Transform(string template, Dictionary<string, string> replaceStrings)
        {
            if (replaceStrings == null)
            {
                return template;
            }
            
            foreach (KeyValuePair<string, string> keyPair in replaceStrings)
            {
                string key = string.Format(KeyFormat, keyPair.Key);
                string value = keyPair.Value ?? string.Empty;

                template = Regex.Replace(template, key, value, RegexOptions.IgnoreCase);
            }           

            return template;
        }

        public virtual List<string> TransformList(ITemplateProvider templateProvider, List<TemplateData> templateData)
        {
            List<string> list = new List<string>(); 
            TemplateCache templateCache = new TemplateCache(templateProvider);

            foreach (TemplateData data in templateData)
            {
                string template = templateCache.ProvideTemplate(data.Variant, data.Culture);
                string content = Transform(template, data.ReplaceModel);
                list.Add(content);
            }

            return list;
        }
    }
}

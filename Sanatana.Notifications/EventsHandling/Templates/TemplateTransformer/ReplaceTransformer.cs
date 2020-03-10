using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sanatana.Notifications.EventsHandling.Templates
{
    public class ReplaceTransformer : ITemplateTransformer
    {
        //properties
        public string KeyFormat { get; set; }
        public bool IgnoreCase { get; set; }


        //init
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyFormat">Use default "{0}" format to replace keys in table as provided. Or append some additional characters like "#{0}" or "{{{0}}}".</param>
        /// <param name="ignoreCase">Ignore case of the key in template.</param>
        public ReplaceTransformer(string keyFormat = "{0}", bool ignoreCase = false)
        {
            KeyFormat = keyFormat;
            IgnoreCase = ignoreCase;
        }
 

        //methods
        public virtual List<string> Transform(ITemplateProvider templateProvider, List<TemplateData> templateData)
        {
            List<string> list = new List<string>(); 
            TemplateCache templateCache = new TemplateCache(templateProvider);

            foreach (TemplateData data in templateData)
            {
                string template = templateCache.GetOrCreateTemplate(data.Culture);
                string content = Transform(template, data.KeyValueModel);
                list.Add(content);
            }

            return list;
        }

        protected virtual string Transform(string template, Dictionary<string, string> replaceStrings)
        {
            if (replaceStrings == null)
            {
                return template;
            }

            foreach (KeyValuePair<string, string> keyPair in replaceStrings)
            {
                string key = string.Format(KeyFormat, keyPair.Key);
                string value = keyPair.Value ?? string.Empty;

                RegexOptions options = IgnoreCase
                    ? RegexOptions.IgnoreCase
                    : RegexOptions.None;
                template = Regex.Replace(template, key, value, options);
            }

            return template;
        }

    }
}

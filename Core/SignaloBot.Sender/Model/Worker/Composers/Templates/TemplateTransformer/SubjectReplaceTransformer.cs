using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Composers.Templates
{
    public class SubjectReplaceTransformer : ITemplateTransformer
    {
        //свойства
        public string KeyFormat { get; set; }

        public int MaxSubjectLength { get; set; }



        //инициализация
        public SubjectReplaceTransformer(string keyFormat = null)
        {
            KeyFormat = keyFormat ?? "{{{0}}}";
            MaxSubjectLength = EmailConstants.EMAIL_MAX_SUBJECT_LENGTH;
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

                int keyOccurance = new Regex(key, RegexOptions.IgnoreCase).Matches(template).Count;
                if (keyOccurance == 0)
                    continue;

                int staticLength = GetStaticLength(template, replaceStrings);
                int maxSubjectLength = MaxSubjectLength < 0
                    ? 0
                    : MaxSubjectLength;
                string valueShortened = StringUtility.ShortenSubjectString(value, staticLength, maxSubjectLength, keyOccurance);

                template = Regex.Replace(template, key, valueShortened, RegexOptions.IgnoreCase);
            }

            return template;
        }

        protected virtual int GetStaticLength(string template, Dictionary<string, string> replaceStrings)
        {
            foreach (KeyValuePair<string, string> keyPair in replaceStrings)
            {
                string key = string.Format(KeyFormat, keyPair.Key);
                template = Regex.Replace(template, key, string.Empty, RegexOptions.IgnoreCase);
            }

            return template.Length;
        }
        
        public virtual List<string> TransformList(
            ITemplateProvider templateProvider, List<TemplateData> templateData)
        {
            List<string> list = new List<string>();
            TemplateCache templateCache = new TemplateCache(templateProvider);

            foreach (TemplateData data in templateData)
            {
                string template = templateCache.GetOrCreateTemplate(data.Culture);
                string content = Transform(template, data.ReplaceModel);
                list.Add(content);
            }

            return list;
        }

    }
}

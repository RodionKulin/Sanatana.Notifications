using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Composing.Templates
{
    public class LimitedLengthReplaceTransformer : ITemplateTransformer
    {
        //properties
        public string KeyFormat { get; set; }
        public int MaxLength { get; set; }
        public bool IgnoreCase { get; set; }



        //init
        public LimitedLengthReplaceTransformer()
        {
            KeyFormat = "{{{0}}}";
            MaxLength = NotificationsConstants.EMAIL_MAX_SUBJECT_LENGTH;
        }

        public LimitedLengthReplaceTransformer(string keyFormat, bool ignoreCase, int maxLength)
        {
            KeyFormat = keyFormat;
            IgnoreCase = ignoreCase;
            MaxLength = maxLength;
        }


        //methods
        public virtual List<string> Transform(ITemplateProvider templateProvider
            , List<TemplateData> templateData)
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

                int keyOccurance = new Regex(key, options).Matches(template).Count;
                if (keyOccurance == 0)
                    continue;

                int staticLength = GetStaticLength(template, replaceStrings);
                int maxSubjectLength = MaxLength < 0 ? 0 : MaxLength;
                string valueShortened = ShortenSubjectString(value, staticLength, maxSubjectLength, keyOccurance);

                template = Regex.Replace(template, key, valueShortened, options);
            }

            return template;
        }

        public virtual string ShortenSubjectString(string partString, int fixedLength, int maxLength
            , int stringRepeatTimes = 1)
        {
            if (stringRepeatTimes < 1)
            {
                throw new Exception($"{stringRepeatTimes} can no be less than 1.");
            }

            int maxLengthForAllParts = (maxLength - fixedLength);
            int maxLengthForEachPart = (int)Math.Floor(maxLengthForAllParts / (decimal)stringRepeatTimes);

            if (partString.Length > maxLengthForEachPart)
            {
                string shortSuffix = "...";

                int newlength = maxLengthForEachPart - shortSuffix.Length;
                if (newlength < 0)
                    newlength = 0;

                return partString.Substring(0, newlength) + shortSuffix;
            }
            else
            {
                return partString;
            }
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

    }
}

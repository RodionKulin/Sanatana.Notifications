using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.EventsHandling.Templates
{
    public class TemplateData : IEquatable<TemplateData>
    {
        //properties
        public Dictionary<string, string> KeyValueModel { get; private set; }
        public object ObjectModel { get; private set; }
        public CultureInfo Culture { get; set; }
        public string Language { get; set; }



        //init
        public TemplateData(Dictionary<string, string> keyValueModel, CultureInfo culture = null, string language = null)
        {
            KeyValueModel = keyValueModel;
            Culture = culture;
            Language = language;
        }

        public TemplateData(object objectModel, CultureInfo culture = null, string language = null)
        {
            ObjectModel = objectModel;
            Culture = culture;
            Language = language;
        }

        public static TemplateData Empty()
        {
            return new TemplateData(new Dictionary<string, string>());
        }




        //convert
        public static implicit operator TemplateData(Dictionary<string, string> keyValueModel)
        {
            return new TemplateData(keyValueModel);
        }


        //IEquatable<TemplateData>
        public bool Equals(TemplateData other)
        {
            if (Language == null && other.Language == null)
            {
                return true;
            }

            return Language == other.Language;
        }
    }
}

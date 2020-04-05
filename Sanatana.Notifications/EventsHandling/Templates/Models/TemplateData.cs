﻿using System;
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
        public string Language { get; set; }



        //init
        public TemplateData(Dictionary<string, string> keyValueModel, string language = null)
        {
            KeyValueModel = keyValueModel;
            Language = language;
        }

        public TemplateData(object objectModel, string language = null)
        {
            ObjectModel = objectModel;
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

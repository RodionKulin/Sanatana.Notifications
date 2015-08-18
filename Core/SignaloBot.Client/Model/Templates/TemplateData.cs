
using SignaloBot.DAL.Entities.Results;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Client.Templates
{
    public class TemplateData
    {
        //свойства
        public Dictionary<string, string> ReplaceModel { get; private set; }
        public object ObjectModel { get; private set; }

        public CultureInfo Culture { get; set; }
        public int Variant { get; set; }



        //инициализация
        public TemplateData(Dictionary<string, string> replaceModel
            , int variant = 0, CultureInfo culture = null)
        {
            ReplaceModel = replaceModel;
            Variant = variant;
            Culture = culture;
        }

        public TemplateData(object objectModel
            , int variant = 0, CultureInfo culture = null)
        {
            ObjectModel = objectModel;
            Variant = variant;
            Culture = culture;
        }

        public static TemplateData Empty()
        {
            return new TemplateData(new Dictionary<string, string>());
        }



        //конверсия
        public static implicit operator TemplateData(Dictionary<string, string> replaceModel)
        {
            return new TemplateData(replaceModel);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Composers.Templates
{
    public class StringTemplate : ITemplateProvider
    {
        //поля
        public string Template { get; set; }

   


        //инициализация
        public StringTemplate(string template)
        {
            Template = template;
        }
      

        //методы
        public virtual string ProvideTemplate(CultureInfo culture = null)
        {
            return Template;
        }
        

        //конверсия
        public static implicit operator StringTemplate(string template)
        {
            return new StringTemplate(template);
        }
    }
}

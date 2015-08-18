using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Client.Templates
{
    public class StringTemplate : ITemplateProvider
    {
        //поля
        public List<string> Templates { get; set; }

        public int VariantsCount
        {
            get { return Templates.Count; }
        }


        //инициализация
        public StringTemplate(string template)
        {
            Templates = new List<string>() { template };
        }
        public StringTemplate(List<string> templates)
        {
            Templates = templates;
        }


        //методы
        public virtual string ProvideTemplate(int variant = 0, CultureInfo culture = null)
        {
            return Templates[variant];
        }
        

        //конверсия
        public static implicit operator StringTemplate(string template)
        {
            return new StringTemplate(template);
        }

        public static implicit operator StringTemplate(List<string> templates)
        {
            return new StringTemplate(templates);
        }
    }
}

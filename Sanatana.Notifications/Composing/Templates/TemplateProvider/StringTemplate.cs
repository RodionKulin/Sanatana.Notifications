using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Composing.Templates
{
    public class StringTemplate : ITemplateProvider
    {
        //properties
        public string Template { get; set; }



        //init
        public StringTemplate(string template)
        {
            Template = template;
        }
      

        //methods
        public virtual string ProvideTemplate(CultureInfo culture = null)
        {
            return Template;
        }
        

        //conversion
        public static implicit operator StringTemplate(string template)
        {
            return new StringTemplate(template);
        }
    }
}

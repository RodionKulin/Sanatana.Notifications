using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Composers.Templates
{
    public interface ITemplateProvider
    {
        string ProvideTemplate(CultureInfo culture = null);
    }
}

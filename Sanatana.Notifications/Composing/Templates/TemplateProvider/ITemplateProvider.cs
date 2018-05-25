using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Composing.Templates
{
    public interface ITemplateProvider
    {
        string ProvideTemplate(CultureInfo culture = null);
    }
}

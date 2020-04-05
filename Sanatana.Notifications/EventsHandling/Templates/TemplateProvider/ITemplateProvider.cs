using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.EventsHandling.Templates
{
    public interface ITemplateProvider
    {
        string ProvideTemplate(string language = null);
    }
}

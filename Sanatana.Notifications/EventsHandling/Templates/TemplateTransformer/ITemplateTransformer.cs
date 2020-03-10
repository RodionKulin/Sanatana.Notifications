using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.EventsHandling.Templates
{
    public interface ITemplateTransformer
    {
        List<string> Transform(ITemplateProvider templateProvider, List<TemplateData> templateData);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.EventsHandling.Templates
{
    public interface ITemplateTransformer
    {
        /// <summary>
        /// Fill templates with provided data for each language.
        /// </summary>
        /// <param name="templateProvider">Template provider</param>
        /// <param name="templateData">Data to replace template placeholders with for each language. Languages should be distinct.</param>
        /// <returns>Dictionary with languages as keys and filled templates as values</returns>
        Dictionary<string, string> Transform(ITemplateProvider templateProvider, List<TemplateData> templateData);
    }
}

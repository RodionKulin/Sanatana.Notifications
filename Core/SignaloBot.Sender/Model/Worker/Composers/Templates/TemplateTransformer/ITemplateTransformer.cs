using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Composers.Templates
{
    public interface ITemplateTransformer
    {
        string Transform(string template, Dictionary<string, string> replaceStrings);
        List<string> TransformList(ITemplateProvider templateProvider, List<TemplateData> templateData);
    }
}

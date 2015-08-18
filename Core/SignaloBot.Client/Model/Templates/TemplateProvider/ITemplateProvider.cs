using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Client.Templates
{
    public interface ITemplateProvider
    {
        int VariantsCount { get; }
        string ProvideTemplate(int variant = 0, CultureInfo culture = null);
    }
}

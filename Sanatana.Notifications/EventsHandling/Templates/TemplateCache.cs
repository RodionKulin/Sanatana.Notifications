using Sanatana.Notifications.EventsHandling.Templates;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.EventsHandling.Templates
{
    public class TemplateCache
    {
        //fields
        private ITemplateProvider _templateProvider;
        private Dictionary<string, object> _cache;


        //init
        public TemplateCache(ITemplateProvider templateProvider)
        {
            _templateProvider = templateProvider;
            _cache = new Dictionary<string, object>();
        }


        //methods
        protected virtual string GetKey(CultureInfo culture = null)
        {
            return culture == null
                ? string.Empty
                : culture.EnglishName;
        }

        public virtual void InsertItem(object item, CultureInfo culture = null)
        {
            string key = GetKey(culture);
            _cache[key] = item;
        }

        public virtual object GetItem(CultureInfo culture = null)
        {
            string key = GetKey(culture);

            if (_cache.ContainsKey(key))
            {
                return _cache[key];
            }
            else
            {
                return null;
            }
        }

        public virtual string GetOrCreateTemplate(CultureInfo culture = null)
        {
            object item = GetItem(culture);

            if (item == null)
            {
                item = _templateProvider.ProvideTemplate(culture);
                InsertItem(item, culture);
            }

            return (string)item;
        }

    }
}

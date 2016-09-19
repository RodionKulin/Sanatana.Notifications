using SignaloBot.Sender.Composers.Templates;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Composers.Templates
{
    public class TemplateCache
    {
        //поля
        ITemplateProvider _templateProvider;
        Dictionary<string, object> _cache;


        //инициализация
        public TemplateCache(ITemplateProvider templateProvider)
        {
            _templateProvider = templateProvider;
            _cache = new Dictionary<string, object>();
        }


        //методы
        public void AddItem(object item, CultureInfo culture = null)
        {
            string key = GetKey(culture);

            if (!_cache.ContainsKey(key))
            {
                _cache.Add(key, item);
            }
        }

        public void InsertItem(object item, CultureInfo culture = null)
        {
            string key = GetKey(culture);

            if (!_cache.ContainsKey(key))
            {
                _cache.Add(key, item);
            }
            else
            {
                _cache[key] = item;
            }
        }

        public string GetOrCreateTemplate(CultureInfo culture = null)
        {
            object item = GetItem(culture);

            if (item == null)
            {
                item = _templateProvider.ProvideTemplate(culture);
                AddItem(item, culture);
            }

            return (string)item;
        }

        public object GetItem(CultureInfo culture = null)
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

        private string GetKey(CultureInfo culture = null)
        {
            string cultureName = culture == null 
                ? null
                : culture.EnglishName;

            return string.Format("{0}", cultureName);
        }
    }
}

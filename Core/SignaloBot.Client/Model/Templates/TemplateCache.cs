using SignaloBot.Client.Templates;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Client.Templates
{
    public class TemplateCache
    {
        ITemplateProvider _templateProvider;
        Dictionary<string, object> _cache;


        //инициализация
        public TemplateCache(ITemplateProvider templateProvider)
        {
            _templateProvider = templateProvider;
            _cache = new Dictionary<string, object>();
        }


        //методы
        public string ProvideTemplate(int variant = 0, CultureInfo culture = null)
        {
            object item = GetItem(variant, culture);

            if (item == null)
            {
                item = _templateProvider.ProvideTemplate(variant, culture);
                AddItem(item, variant, culture);
            }

            return (string)item;
        }

        public void AddItem(object item, int variant = 0, CultureInfo culture = null)
        {
            string key = GetKey(variant, culture);

            if (!_cache.ContainsKey(key))
            {
                _cache.Add(key, item);
            }
        }

        public void InsertItem(object item, int variant = 0, CultureInfo culture = null)
        {
            string key = GetKey(variant, culture);

            if (!_cache.ContainsKey(key))
            {
                _cache.Add(key, item);
            }
            else
            {
                _cache[key] = item;
            }
        }

        public object GetItem(int variant = 0, CultureInfo culture = null)
        {
            string key = GetKey(variant, culture);

            if (_cache.ContainsKey(key))
            {
                return _cache[key];
            }
            else
            {
                return null;
            }
        }

        private string GetKey(int variant = 0, CultureInfo culture = null)
        {
            string cultureName = culture == null 
                ? null
                : culture.EnglishName;

            return string.Format("{0}-{1}", variant, cultureName);
        }
    }
}

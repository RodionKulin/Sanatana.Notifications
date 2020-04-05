using Sanatana.Notifications.EventsHandling.Templates;
using System;
using System.Collections.Concurrent;
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
        private ConcurrentDictionary<string, object> _cache;


        //init
        public TemplateCache()
        {
            _cache = new ConcurrentDictionary<string, object>();
        }


        //methods
        public virtual object GetOrCreate(string language, Func<object> itemCreator)
        {
            language = language ?? string.Empty;
            return _cache.GetOrAdd(language, _ => itemCreator());
        }

    }
}

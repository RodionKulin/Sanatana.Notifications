using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.EventsHandling.Templates
{
    public class FileTemplate : ITemplateProvider
    {
        //fields
        protected static string _longTermTemplatesCache;


        //properties
        public string FilePath { get; set; }
        /// <summary>
        /// Cache file content in memory. Default is true.
        /// </summary>
        public bool LongTermCache { get; set; } = true;
              


        //init      
        public FileTemplate(string templatePath)
        {
            FilePath = templatePath;
        }


        //methods
        public virtual string ProvideTemplate(string language = null)
        {
            if (!LongTermCache)
            {
                return File.ReadAllText(FilePath);
            }

            if (_longTermTemplatesCache == null)
            {
                _longTermTemplatesCache = File.ReadAllText(FilePath);
            }
            return _longTermTemplatesCache;
        }

    }
}

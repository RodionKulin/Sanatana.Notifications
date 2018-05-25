using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Composing.Templates
{
    public class FileTemplate : ITemplateProvider
    {
        //properties
        public string FilePath { get; set; }
              


        //init      
        public FileTemplate(string templatePath)
        {
            FilePath = templatePath;
        }


        //methods
        public virtual string ProvideTemplate(CultureInfo culture = null)
        {
            string text;
           
            using (var fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var streamReader = new StreamReader(fileStream))
            {
                text = streamReader.ReadToEnd();
            }

            return text;
        }

    }
}

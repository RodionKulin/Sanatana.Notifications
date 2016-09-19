using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Composers.Templates
{
    public class FileTemplate : ITemplateProvider
    {
        //свойства
        public string FilePath { get; set; }
              


        //инициализация      
        public FileTemplate(string templatePath)
        {
            FilePath = templatePath;
        }


        //методы
        public virtual string ProvideTemplate(CultureInfo culture = null)
        {
            string text;
           
            using (FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader streamReader = new StreamReader(fileStream))
            {
                text = streamReader.ReadToEnd();
            }

            return text;
        }

    }
}

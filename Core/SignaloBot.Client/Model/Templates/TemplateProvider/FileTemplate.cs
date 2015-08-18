using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Client.Templates
{
    public class FileTemplate : ITemplateProvider
    {
        //свойства
        public List<string> FilePaths { get; set; }

        public int VariantsCount
        {
            get { return FilePaths.Count; }
        }
       


        //инициализация
        public FileTemplate(string template)
        {
            FilePaths = new List<string>() { template };
        }
        public FileTemplate(List<string> templates)
        {
            FilePaths = templates;
        }


        //методы
        public virtual string ProvideTemplate(int variant = 0, CultureInfo culture = null)
        {
            string text;
            string path = FilePaths[variant];
           
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader streamReader = new StreamReader(fileStream))
            {
                text = streamReader.ReadToEnd();
            }

            return text;
        }

    }
}

using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Client.Templates
{
    internal class RazorTemplateManager : ITemplateManager
    {
        string _templateFolder;


        //инициализация
        public RazorTemplateManager(string razorTemplatesRelativeFolder)
        {
            _templateFolder = razorTemplatesRelativeFolder;
        }


        //методы
        public ITemplateSource Resolve(ITemplateKey key)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _templateFolder, key.Name);
            string template = File.ReadAllText(filePath);

            // Provide a non-null file to improve debugging
            return new LoadedTemplateSource(template, filePath);
        }

        public ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context)
        {
            // If you can have different templates with the same name depending on the 
            // context or the resolveType you need your own implementation here!
            // Otherwise you can just use NameOnlyTemplateKey.
            ITemplateKey tk = new NameOnlyTemplateKey(name, resolveType, context);
            return tk;
        }

        public void AddDynamic(ITemplateKey key, ITemplateSource source)
        {
            // You can disable dynamic templates completely, but 
            // then all convenience methods (Compile and RunCompile) with
            // a TemplateSource will no longer work (they are not really needed anyway).
            throw new NotImplementedException("dynamic templates are not supported!");
        }
    }
}

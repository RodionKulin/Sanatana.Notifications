using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace Sanatana.Notifications.EventsHandling.Templates
{
    public class XsltTransformer : ITemplateTransformer
    {
        //methods
        public virtual string Transform(string template, TemplateData data)
        {
            XslCompiledTransform transform = ConstructXslTransform(template);

            string content = null;
            if (data.ObjectModel != null)
            {
                content = ApplyTransform(transform, data.ObjectModel);
            }
            else
            {
                var replaceModel = data.KeyValueModel ?? new Dictionary<string, string>();
                XsltArgumentList argList = ConstructArgumentList(replaceModel);
                content = ApplyTransform(transform, argList);
            }

            return content;
        }

        public virtual List<string> Transform(ITemplateProvider templateProvider, List<TemplateData> templateData)
        {
            List<string> list = new List<string>();
            TemplateCache templateCache = new TemplateCache(templateProvider);

            foreach (TemplateData data in templateData)
            {
                var transform = (XslCompiledTransform)templateCache.GetItem(data.Culture);
                if (transform == null)
                {
                    string template = templateProvider.ProvideTemplate(data.Culture);
                    transform = ConstructXslTransform(template);
                    templateCache.InsertItem(transform, data.Culture);
                }

                string content = null;
                if (data.ObjectModel != null)
                {
                    content = ApplyTransform(transform, data.ObjectModel);
                }
                else
                {
                    var replaceModel = data.KeyValueModel ?? new Dictionary<string, string>();
                    XsltArgumentList argList = ConstructArgumentList(replaceModel);
                    content = ApplyTransform(transform, argList);
                }

                list.Add(content);
            }

            return list;
        }

        protected virtual XsltArgumentList ConstructArgumentList(Dictionary<string, string> parameters)
        {
            XsltArgumentList arguments = new XsltArgumentList();

            foreach (string key in parameters.Keys)
            {
                arguments.AddParam(key, "", parameters[key]);
            }

            return arguments;
        }

        protected virtual XslCompiledTransform ConstructXslTransform(string template)
        {
            XslCompiledTransform xslTransformer = new XslCompiledTransform();

            using (TextReader textReader = new StringReader(template))
            using (XmlReader styleSheetReader = XmlReader.Create(textReader))
            {                
                xslTransformer.Load(styleSheetReader);
            }

            return xslTransformer;
        }

        protected virtual string ApplyTransform(XslCompiledTransform xslTransformer, object objectModel)
        {
            StringBuilder outputString = new StringBuilder();

            using (MemoryStream templateDataStream = new MemoryStream())
            {
                XmlSerializer serializer = new XmlSerializer(objectModel.GetType());
                serializer.Serialize(templateDataStream, objectModel);
                templateDataStream.Position = 0;

                XmlWriterSettings writerSettings = new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true,
                    ConformanceLevel = ConformanceLevel.Fragment
                };

                using (XmlReader reader = XmlReader.Create(templateDataStream))
                using (StringWriter outputStringWriter = new StringWriter(outputString))
                using (XmlWriter writer = XmlWriter.Create(outputStringWriter, writerSettings))
                {
                    xslTransformer.Transform(reader, writer);
                }
            }

            return outputString.ToString();
        }

        protected virtual string ApplyTransform(XslCompiledTransform xslTransformer, XsltArgumentList argList)
        {
            string emptyXml = "<root></root>";
            StringBuilder outputString = new StringBuilder();

            XmlWriterSettings writerSettings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment
            };

            using (StringReader stringReader = new StringReader(emptyXml))
            using (XmlReader xmlReader = XmlReader.Create(stringReader))
            using (StringWriter outputStringWriter = new StringWriter(outputString))
            using (XmlWriter writer = XmlWriter.Create(outputStringWriter, writerSettings))
            {
                xslTransformer.Transform(xmlReader, argList, writer);
            }

            return outputString.ToString();
        }




    }
}

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
        public virtual Dictionary<TemplateData, string> Transform(ITemplateProvider templateProvider, List<TemplateData> templateData)
        {
            if (templateProvider == null)
            {
                return new Dictionary<TemplateData, string>();
            }

            TemplateCache templateCache = new TemplateCache(templateProvider);
            return templateData.ToDictionary(data => data, data =>
            {
                XslCompiledTransform transform = ConstructXslTransform(templateProvider, templateCache, data);
                return data.ObjectModel == null
                    ? ApplyXslt(transform, data.KeyValueModel)
                    : ApplyXslt(transform, data.ObjectModel);
            });
        }

        protected virtual XslCompiledTransform ConstructXslTransform(ITemplateProvider templateProvider, TemplateCache templateCache, TemplateData data)
        {
            var transform = (XslCompiledTransform)templateCache.GetItem(data.Culture);
            if (transform != null)
            {
                return transform;
            }

            string template = templateProvider.ProvideTemplate(data.Culture);

            transform = new XslCompiledTransform();
            using (TextReader textReader = new StringReader(template))
            using (XmlReader styleSheetReader = XmlReader.Create(textReader))
            {                
                transform.Load(styleSheetReader);
            }

            templateCache.InsertItem(transform, data.Culture);
            return transform;
        }

        protected virtual string ApplyXslt(XslCompiledTransform xslTransformer, object objectModel)
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

        protected virtual string ApplyXslt(XslCompiledTransform xslTransformer, Dictionary<string, string> keyValues)
        {
            keyValues = keyValues ?? new Dictionary<string, string>();
            XsltArgumentList argList = ConstructArgumentList(keyValues);

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

        protected virtual XsltArgumentList ConstructArgumentList(Dictionary<string, string> parameters)
        {
            XsltArgumentList arguments = new XsltArgumentList();
            foreach (string key in parameters.Keys)
            {
                arguments.AddParam(key, "", parameters[key]);
            }

            return arguments;
        }

    }
}

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
        //fields
        protected TemplateCache _longTermTemplatesCache = new TemplateCache();


        //properties
        /// <summary>
        /// Create XslCompiledTransform only once and reuse on next calls. Default is true. 
        /// Memory Cache is persistent if EventSettings are stored in memory.
        /// But will be wiped out on EventSettings reading if EventSettings are stored in database.
        /// </summary>
        public bool UseLongTermCaching { get; set; } = true;



        //methods
        public virtual Dictionary<string, string> Transform(ITemplateProvider templateProvider, List<TemplateData> templateData)
        {
            if (templateProvider == null)
            {
                throw new ArgumentNullException(nameof(templateProvider));
            }

            return templateData.ToDictionary(data => data.Language ?? string.Empty, data =>
            {
                var transform = UseLongTermCaching
                    ? (XslCompiledTransform)_longTermTemplatesCache.GetOrCreate(data.Language, () => ConstructXslTransform(templateProvider, data))
                    : ConstructXslTransform(templateProvider, data);

                return data.ObjectModel == null
                    ? ApplyXslt(transform, data.KeyValueModel)
                    : ApplyXslt(transform, data.ObjectModel);
            });
        }

        protected virtual XslCompiledTransform ConstructXslTransform(ITemplateProvider templateProvider, TemplateData data)
        {
            string template = templateProvider.ProvideTemplate(data.Language);

            var transform = new XslCompiledTransform();
            using (TextReader textReader = new StringReader(template))
            using (XmlReader styleSheetReader = XmlReader.Create(textReader))
            {                
                transform.Load(styleSheetReader);
            }

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

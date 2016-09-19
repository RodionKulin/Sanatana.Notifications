using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using Common.Utility;

namespace SignaloBot.Sender.Composers.Templates
{
    public class XslTransformer : ITemplateTransformer
    {
        //методы
        public virtual XslCompiledTransform ConstructXslTransform(string template)
        {
            XslCompiledTransform xslTransformer = new XslCompiledTransform();

            using (TextReader textReader = new StringReader(template))
            using (XmlReader styleSheetReader = XmlReader.Create(textReader))
            {                
                xslTransformer.Load(styleSheetReader);
            }

            return xslTransformer;
        }

        public virtual XsltArgumentList ConstructArgumentList(Dictionary<string, string> parameters)
        {
            XsltArgumentList arguments = new XsltArgumentList();

            foreach (string key in parameters.Keys)
            {
                arguments.AddParam(key, "", parameters[key]);
            }

            return arguments;
        }

        public virtual string Transform(string template, Dictionary<string, string> replaceModel)
        {
            XslCompiledTransform xslTransform = ConstructXslTransform(template);
            XsltArgumentList argList = ConstructArgumentList(replaceModel);
            return Transform(xslTransform, argList);
        }

        public virtual string Transform(XslCompiledTransform xslTransformer, object objectModel)
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

        public virtual string Transform(XslCompiledTransform xslTransformer, XsltArgumentList argList)
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



        public virtual List<string> TransformList(
            ITemplateProvider templateProvider, List<TemplateData> templateData)
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
                    content = Transform(transform, data.ObjectModel);
                }
                else
                {
                    var replaceModel = data.ReplaceModel ?? new Dictionary<string, string>();
                    content = Transform(transform, replaceModel);
                }

                list.Add(content);
            }

            return list;
        }


    }
}

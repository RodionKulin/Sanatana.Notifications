using System;
using System.Xml.Serialization;

namespace TemplatesExample
{
    [Serializable]
    [XmlRoot("product")]
    public class Product
    {
        private int mId;
        private string mName;
        private decimal mPrice;

        [XmlAttribute("id")]
        public int Id
        {
            get { return mId; }
            set { mId = value; }
        }

        [XmlElement("name")]
        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        [XmlElement("price")]
        public decimal Price
        {
            get { return mPrice; }
            set { mPrice = value; }
        }
    }
}
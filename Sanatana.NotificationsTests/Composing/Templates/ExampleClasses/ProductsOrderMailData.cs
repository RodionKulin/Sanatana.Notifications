using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TemplatesExample
{
    [Serializable]
    [XmlRoot("mailData")]
    public class ProductsOrderMailData
    {
        private string mCustomerName;
        private int mOrderId;
        private DateTime mOrderDate;
        private string mFormattedOrderDate;
        private List<Product> mProducts = new List<Product>();

        [XmlElement("name")]
        public string CustomerName
        {
            get { return mCustomerName; }
            set { mCustomerName = value; }
        }

        [XmlAttribute("orderId")]
        public int OrderId
        {
            get { return mOrderId; }
            set { mOrderId = value; }
        }

        [XmlIgnore]
        public DateTime OrderDate
        {
            get { return mOrderDate; }
            set
            {
                mOrderDate = value;
                mFormattedOrderDate = mOrderDate.ToString("dd MMM yyyy HH:mm");
            }
        }

        [XmlElement("orderDate")]
        public string FormattedOrderDate
        {
            get { return mFormattedOrderDate; }
            set { mFormattedOrderDate = value; }
        }

        [XmlArray("orderedProducts")]
        [XmlArrayItem("orderedProduct")]
        public List<Product> Products
        {
            get { return mProducts; }
        }
    }
}
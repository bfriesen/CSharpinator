using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSharpifier
{
    [XmlRoot("Attribute")]
    public class AttributeProxy
    {
        public string AttributeTypeName { get; set; }
        public string ElementNameSetter { get; set; }

        public static AttributeProxy XmlElement(string elementName)
        {
            return new AttributeProxy { AttributeTypeName = "XmlElement", ElementNameSetter = elementName };
        }

        public static AttributeProxy XmlAttribute(string attributeName)
        {
            return new AttributeProxy { AttributeTypeName = "XmlAttribute", ElementNameSetter = attributeName };
        }

        public static AttributeProxy XmlText()
        {
            return new AttributeProxy { AttributeTypeName = "XmlText" };
        }

        public string ToCode()
        {
            return ElementNameSetter != null
                ? string.Format("[{0}(\"{1}\")]", AttributeTypeName, ElementNameSetter)
                : string.Format("[{0}]", AttributeTypeName);
        }
    }
}

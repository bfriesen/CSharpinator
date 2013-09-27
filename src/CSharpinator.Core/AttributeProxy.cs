using System.Xml.Serialization;

namespace CSharpinator
{
    [XmlRoot("Attribute")]
    public class AttributeProxy
    {
        public string AttributeTypeName { get; set; }
        public string ElementNameSetter { get; set; }

        public static AttributeProxy XmlElement(string elementName)
        {
            return new AttributeProxy { AttributeTypeName = "XmlElement", ElementNameSetter = string.Format("\"{0}\"", elementName) };
        }

        public static AttributeProxy XmlAttribute(string attributeName)
        {
            return new AttributeProxy { AttributeTypeName = "XmlAttribute", ElementNameSetter = string.Format("\"{0}\"", attributeName) };
        }

        public static AttributeProxy XmlText()
        {
            return new AttributeProxy { AttributeTypeName = "XmlText" };
        }

        public static AttributeProxy XmlArray(string arrayName)
        {
            return new AttributeProxy { AttributeTypeName = "XmlArray", ElementNameSetter = string.Format("\"{0}\"", arrayName) };
        }

        public static AttributeProxy XmlArrayItem(string arrayItemName)
        {
            return new AttributeProxy { AttributeTypeName = "XmlArrayItem", ElementNameSetter = string.Format("\"{0}\"", arrayItemName) };
        }

        public static AttributeProxy DataMember(string name)
        {
            return new AttributeProxy { AttributeTypeName = "DataMember", ElementNameSetter = string.Format(@"Name=""{0}""", name) };
        }

        public string ToCode()
        {
            return ElementNameSetter != null
                ? string.Format("[{0}({1})]", AttributeTypeName, ElementNameSetter)
                : string.Format("[{0}]", AttributeTypeName);
        }
    }
}

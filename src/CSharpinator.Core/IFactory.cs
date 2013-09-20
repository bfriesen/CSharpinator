using System.Collections.Generic;
using System.Xml.Linq;

namespace CSharpinator
{
    public interface IFactory
    {
        XmlDomElement CreateXmlDomElement(XElement element);
        XmlDomAttribute CreateXmlDomAttribute(XAttribute attribute);
        XmlDomText CreateXmlDomText(XText text);

        Property CreateProperty(string id, bool isNonEmpty);
        PropertyDefinition CreatePropertyDefinition(IClass @class, string propertyName, bool isLegal, bool isEnabled, params AttributeProxy[] attributes);

        IEnumerable<IBclClass> GetAllBclClasses();
        IBclClass GetBclClassFromTypeName(string typeName);

        FormattedDateTime GetOrCreateFormattedDateTime(string format);

        NullableFormattedDateTime GetOrCreateNullableFormattedDateTime(string format);

        DomPath GetOrCreateDomPath(string fullPath);
        DomPath GetOrCreateDomPath(string fullPath, int typeNameDepth);
    }
}
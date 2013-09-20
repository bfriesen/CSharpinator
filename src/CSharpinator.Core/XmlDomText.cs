using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CSharpinator
{
    public class XmlDomText : IDomElement
    {
        private readonly string _value;
        private readonly XText _text;
        private readonly IFactory _factory;

        public XmlDomText(XText text, IFactory factory)
        {
            _text = text;
            _value = text.Value;
            _factory = factory;
        }

        public bool HasElements
        {
            get { return false; }
        }

        public string Value
        {
            get { return _value; }
        }

        public string Name
        {
            get { return "Value"; }
        }

        public IEnumerable<IDomElement> Elements
        {
            get
            {
                yield break;
            }
        }

        public Property CreateProperty(IClassRepository classRepository)
        {
            var property = _factory.CreateProperty(Name, !string.IsNullOrEmpty(_value));
            
            property.InitializeDefaultPropertyDefinitionSet(
                propertyDefinitions =>
                propertyDefinitions.Append(
                _factory.GetAllBclClasses()
                    .Select(bclClass =>
                        _factory.CreatePropertyDefinition(bclClass, Name, bclClass.IsLegalValue(_value), true, AttributeProxy.XmlText()))));

            return property;
        }

        public DomPath GetDomPath(IFactory factory)
        {
            return _text.GetDomPath(factory);
        }
    }
}

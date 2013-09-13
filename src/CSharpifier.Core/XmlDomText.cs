using System.Collections.Generic;
using System.Linq;

namespace CSharpifier
{
    public class XmlDomText : IDomElement
    {
        private readonly string _value;

        public XmlDomText(string value)
        {
            _value = value;
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
            var property = new Property(Name, !string.IsNullOrEmpty(_value));
            
            property.InitializePotentialPropertyDefinitions(
                propertyDefinitions =>
                propertyDefinitions.Append(
                BclClass.All
                    .Select(bclClass =>
                        new PropertyDefinition(bclClass, Name, bclClass.IsLegalValue(_value), true)
                        {
                            Attributes = new List<AttributeProxy> { AttributeProxy.XmlText() }
                        })));

            return property;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CSharpinator
{
    public class JValueDomElement : IDomElement
    {
        private readonly JValue _jValue;
        private readonly string _name;
        private readonly IFactory _factory;

        public JValueDomElement(JValue jValue, string name, IFactory factory)
        {
            _jValue = jValue;
            _name = name;
            _factory = factory;
        }

        public bool HasElements
        {
            get { return false; }
        }

        public bool ActsAsRootElement
        {
            get { return false; }
        }

        public IEnumerable<IDomElement> Elements
        {
            get { yield break; }
        }

        public Property CreateProperty(IClassRepository classRepository)
        {
            var property = _factory.CreateProperty(
                _jValue.GetDomPath(_factory),
                _jValue.Value != null && (!(_jValue.Value is string) || (string)_jValue.Value != ""));

            property.InitializeDefaultPropertyDefinitionSet(
                propertyDefinitions =>
                propertyDefinitions.Append(
                    _factory.GetAllBclClasses()
                        .Select(bclClass =>
                            _factory.CreatePropertyDefinition(bclClass, _name, bclClass.IsLegalObjectValue(_jValue.Value), true, AttributeProxy.DataMember(_name)))));

            return property;
        }

        public DomPath GetDomPath(IFactory factory)
        {
            return _jValue.GetDomPath(factory);
        }
    }
}
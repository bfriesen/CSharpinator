using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CSharpinator
{
    public class JObjectDomElement : IDomElement
    {
        private readonly JObject _jObject;
        private readonly string _name;
        private readonly IFactory _factory;

        public JObjectDomElement(JObject jObject, string name, IFactory factory)
        {
            _jObject = jObject;
            _name = name;
            _factory = factory;
        }

        public bool HasElements
        {
            get {  return true; }
        }

        public IEnumerable<IDomElement> Elements
        {
            get
            {
                return _jObject.Properties().Select(property => _factory.CreateJsonDomElement(property));
            }
        }

        public Property CreateProperty(IClassRepository classRepository)
        {
            var property = _factory.CreateProperty(_jObject.GetDomPath(_factory), _jObject.Properties().Any());

            // TODO: call property.InitializeDefaultPropertyDefinitionSet (look at XmlDomElement for an example)

            throw new NotImplementedException("TODO: call property.InitializeDefaultPropertyDefinitionSet (look at XmlDomElement for an example)");
            return property;
        }

        public DomPath GetDomPath(IFactory factory)
        {
            return _jObject.GetDomPath(factory);
        }
    }
}
﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace CSharpinator
{
    [DebuggerDisplay("{Name}")]
    public class XmlDomAttribute : IDomElement
    {
        private readonly XAttribute _attribute;
        private readonly IFactory _factory;

        public XmlDomAttribute(XAttribute attribute, IFactory factory)
        {
            _attribute = attribute;
            _factory = factory;
        }

        public bool HasElements
        {
            get { return false; }
        }

        public string Value
        {
            get { return _attribute.Value; }
        }

        public string Name
        {
            get { return _attribute.Name.ToString(); }
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
            var property = _factory.CreateProperty(_attribute.Name.ToString(), !string.IsNullOrEmpty(_attribute.Value));
            
            property.InitializeDefaultPropertyDefinitionSet(
                propertyDefinitions =>
                propertyDefinitions.Append(
                    _factory.GetAllBclClasses()
                        .Select(bclClass =>
                            _factory.CreatePropertyDefinition(bclClass, _attribute.Name.ToString(), bclClass.IsLegalValue(_attribute.Value), true, AttributeProxy.XmlAttribute(_attribute.Name.ToString())))));
            
            return property;
        }

        public DomPath GetDomPath(IFactory factory)
        {
            return _attribute.GetDomPath(factory);
        }
    }
}
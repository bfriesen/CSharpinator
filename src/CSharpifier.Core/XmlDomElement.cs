using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace CSharpifier
{
    [DebuggerDisplay("{Name}")]
    public class XmlDomElement : IDomElement
    {
        private static readonly Lazy<PluralizationService> _pluralizationService = new Lazy<PluralizationService>(() => PluralizationService.CreateService(new CultureInfo("en")));
        private readonly XElement _element;

        public XmlDomElement(XElement element)
        {
            _element = element;
        }

        public bool HasElements
        {
            get { return _element.HasElements || _element.HasAttributes || !string.IsNullOrEmpty(_element.Value); }
        }

        public string Value
        {
            get { return _element.Value; }
        }

        public string Name
        {
            get { return _element.Name.ToString(); }
        }

        public IEnumerable<IDomElement> Elements
        {
            get
            {
                return
                    _element.Attributes().Select(x => (IDomElement)new XmlDomAttribute(x))
                        .Concat(_element.Elements().Select(x => new XmlDomElement(x)))
                        .Concat(
                            !_element.HasElements && !string.IsNullOrEmpty(_element.Value)
                                ? new[] { new XmlDomText(_element.Value) }
                                : Enumerable.Empty<IDomElement>());
            }
        }

        public Property CreateProperty(IClassRepository classRepository)
        {
            var property = new Property(_element.Name.ToString(), _element.HasElements || _element.HasAttributes || !string.IsNullOrEmpty(_element.Value));

            property.InitializeDefaultPropertyDefinitionSet(
                propertyDefinitions =>
                {
                    if (!_element.HasElements && !_element.HasAttributes)
                    {
                        propertyDefinitions.Append(
                            BclClass.All
                                .Select(bclClass =>
                                    new PropertyDefinition(bclClass, _element.Name.ToString(), bclClass.IsLegalValue(_element.Value), true)
                                    {
                                        Attributes = new List<AttributeProxy> { AttributeProxy.XmlElement(_element.Name.ToString()) }
                                    }));
                    }
                    else
                    {
                        propertyDefinitions.Append(
                            BclClass.All
                                .Select(bclClass =>
                                    new PropertyDefinition(bclClass, _element.Name.ToString(), false, true)
                                    {
                                        Attributes = new List<AttributeProxy> { AttributeProxy.XmlElement(_element.Name.ToString()) }
                                    }));
                    }

                    var userDefinedClassPropertyDefinition =
                        new PropertyDefinition(classRepository.GetOrAdd(_element.Name.ToString()), _element.Name.ToString(), true, true)
                        {
                            Attributes = new List<AttributeProxy> { AttributeProxy.XmlElement(_element.Name.ToString()) }
                        };

                    if (_element.HasElements || _element.HasAttributes)
                    {
                        propertyDefinitions.Prepend(userDefinedClassPropertyDefinition);
                    }
                    else
                    {
                        propertyDefinitions.Append(userDefinedClassPropertyDefinition);
                    }

                    if (!_element.HasAttributes && _element.HasElements)
                    {
                        var first = _element.Elements().First();

                        if (_element.Elements().Skip(1).All(x => x.Name == first.Name))
                        {
                            var xmlArraySet = property.GetOrAddExtraPropertyDefinitionSet("xml_array_list");
                            xmlArraySet.IsEnabled = true;

                            var xmlArrayListPropertyDefinition =
                                    new PropertyDefinition(
                                        ListClass.FromClass(classRepository.GetOrAdd(first.Name.ToString())),
                                        _pluralizationService.Value.Pluralize(first.Name.ToString()),
                                        true,
                                        true)
                                    {
                                        Attributes = new List<AttributeProxy>
                                    {
                                        AttributeProxy.XmlArray(_element.Name.ToString()),
                                        AttributeProxy.XmlArrayItem(first.Name.ToString())
                                    }
                                    };

                            xmlArraySet.PropertyDefinitions = new List<PropertyDefinition> { xmlArrayListPropertyDefinition };

                            if (_element.Elements().Count() > 1)
                            {
                                xmlArraySet.Order = -1;
                            }
                            else
                            {
                                xmlArraySet.Order = 2;
                            }
                        }
                    }
                    else
                    {
                        if (property.ExtraPropertyDefinitionSetExists("xml_array_list"))
                        {
                            var xmlArraySet = property.GetOrAddExtraPropertyDefinitionSet("xml_array_list");
                            xmlArraySet.IsEnabled = false;
                        }
                    }

                    var xmlElementSet = property.GetOrAddExtraPropertyDefinitionSet("xml_element_list");

                    var xmlElementListPropertyDefinitions = propertyDefinitions
                        .Where(x => !(x.Class is ListClass))
                        .Select(x =>
                            new PropertyDefinition(
                                ListClass.FromClass(x.Class),
                                _pluralizationService.Value.Pluralize(_element.Name.ToString()),
                                x.IsLegal,
                                x.IsEnabled)
                            {
                                Attributes = new List<AttributeProxy> { AttributeProxy.XmlElement(_element.Name.ToString()) }
                            }
                        ).ToList();

                    xmlElementSet.PropertyDefinitions = xmlElementListPropertyDefinitions;

                    if (_element.Parent != null)
                    {
                        xmlElementSet.IsEnabled = true;

                        if (_element.Parent.Elements(_element.Name).Count() > 1)
                        {
                            xmlElementSet.Order = -2;
                        }
                        else
                        {
                            xmlElementSet.Order = 1;
                        }
                    }
                    else
                    {
                        xmlElementSet.IsEnabled = false;
                    }
                });

            return property;
        }
    }
}

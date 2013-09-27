using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace CSharpinator
{
    [DebuggerDisplay("{_element.Name.ToString()}")]
    public class XmlDomElement : IDomElement
    {
        private static readonly Lazy<PluralizationService> _pluralizationService = new Lazy<PluralizationService>(() => PluralizationService.CreateService(new CultureInfo("en")));
        private readonly XElement _element;
        private readonly IFactory _factory;

        public XmlDomElement(XElement element, IFactory factory)
        {
            _element = element;
            _factory = factory;
        }

        public bool HasElements
        {
            get { return _element.HasElements || _element.HasAttributes || !string.IsNullOrEmpty(_element.Value); }
        }

        public IEnumerable<IDomElement> Elements
        {
            get
            {
                return
                    _element.Attributes().Select(x => (IDomElement)_factory.CreateXmlDomAttribute(x))
                        .Concat(_element.Elements().Select(x => _factory.CreateXmlDomElement(x)))
                        .Concat(
                            !_element.HasElements && !string.IsNullOrEmpty(_element.Value)
                                ? new[] { _factory.CreateXmlDomText((XText)_element.Nodes().First()) }
                                : Enumerable.Empty<IDomElement>());
            }
        }

        public Property CreateProperty(IClassRepository classRepository)
        {
            var property = _factory.CreateProperty(_element.GetDomPath(_factory), _element.HasElements || _element.HasAttributes || !string.IsNullOrEmpty(_element.Value));

            property.InitializeDefaultPropertyDefinitionSet(
                propertyDefinitions =>
                {
                    if (!_element.HasElements && !_element.HasAttributes)
                    {
                        propertyDefinitions.Append(
                            _factory.GetAllBclClasses()
                                .Select(bclClass =>
                                    _factory.CreatePropertyDefinition(bclClass, _element.Name.ToString(), bclClass.IsLegalValue(_element.Value), true, AttributeProxy.XmlElement(_element.Name.ToString()))));
                    }
                    else
                    {
                        propertyDefinitions.Append(
                            _factory.GetAllBclClasses()
                                .Select(bclClass =>
                                    _factory.CreatePropertyDefinition(bclClass, _element.Name.ToString(), false, true, AttributeProxy.XmlElement(_element.Name.ToString()))));
                    }

                    var userDefinedClassPropertyDefinition =
                        _factory.CreatePropertyDefinition(classRepository.GetOrAdd(_element.GetDomPath(_factory)), _element.Name.ToString(), true, true, AttributeProxy.XmlElement(_element.Name.ToString()));

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
                                    _factory.CreatePropertyDefinition(
                                        ListClass.FromClass(classRepository.GetOrAdd(first.GetDomPath(_factory))),
                                        _pluralizationService.Value.Pluralize(first.Name.ToString()),
                                        true,
                                        true,
                                        AttributeProxy.XmlArray(_element.Name.ToString()),
                                        AttributeProxy.XmlArrayItem(first.Name.ToString()));

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
                        else
                        {
                            if (property.ExtraPropertyDefinitionSetExists("xml_array_list"))
                            {
                                var xmlArraySet = property.GetOrAddExtraPropertyDefinitionSet("xml_array_list");
                                xmlArraySet.IsEnabled = false;
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
                            _factory.CreatePropertyDefinition(
                                ListClass.FromClass(x.Class),
                                _pluralizationService.Value.Pluralize(_element.Name.ToString()),
                                x.IsLegal,
                                x.IsEnabled,
                                AttributeProxy.XmlElement(_element.Name.ToString()))
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

        public DomPath GetDomPath(IFactory factory)
        {
            return _element.GetDomPath(factory);
        }
    }
}

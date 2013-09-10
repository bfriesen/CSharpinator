using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CSharpifier
{
    public class XmlDomElement : IDomElement
    {
        private static Lazy<PluralizationService> _pluralizationService = new Lazy<PluralizationService>(() => PluralizationService.CreateService(new CultureInfo("en")));
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
            var property = new Property(_element.Name);

            if (!_element.HasElements && !_element.HasAttributes)
            {
                Console.WriteLine(_element.Name + ": potential bcl properties");

                property.AppendPotentialPropertyDefinitions(
                    BclClass.GetLegalClassesFromValue(_element.Value)
                        .Select(bclClass =>
                            new PropertyDefinition(bclClass, _element.Name.ToString())
                            {
                                Attributes = new List<AttributeProxy> { AttributeProxy.XmlElement(_element.Name.ToString()) }
                            }));
            }
            else
            {
                Console.WriteLine(_element.Name + ": no potential bcl properties");
            }

            var userDefinedClassPropertyDefinition =
                new PropertyDefinition(classRepository.GetOrCreate(_element.Name.ToString()), _element.Name.ToString())
                {
                    Attributes = new List<AttributeProxy> { AttributeProxy.XmlElement(_element.Name.ToString()) }
                };

            if (_element.HasElements || _element.HasAttributes)
            {
                property.PrependPotentialPropertyDefinition(userDefinedClassPropertyDefinition);
                Console.WriteLine(_element.Name + ": potential high-priority udc property");
            }
            else
            {
                property.AppendPotentialPropertyDefinition(userDefinedClassPropertyDefinition);
                Console.WriteLine(_element.Name + ": potential low-priority udc property");
            }

            if (!_element.HasAttributes && _element.HasElements)
            {
                var first = _element.Elements().First();
                if (_element.Elements().Skip(1).All(x => x.Name == first.Name))
                {
                    var listPropertyDefinition =
                        new PropertyDefinition(ListClass.FromClass(classRepository.GetOrCreate(first.Name.ToString())), _pluralizationService.Value.Pluralize(first.Name.ToString()))
                        {
                            Attributes = new List<AttributeProxy>
                            {
                                AttributeProxy.XmlArray(_element.Name.ToString()),
                                AttributeProxy.XmlArrayItem(first.Name.ToString())
                            }
                        };

                    if (_element.Elements().Count() > 1)
                    {
                        property.PrependPotentialPropertyDefinition(listPropertyDefinition);
                        Console.WriteLine(_element.Name + ": potential high-priority XmlArray/XmlArrayItem list");
                    }
                    else
                    {
                        property.AppendPotentialPropertyDefinition(listPropertyDefinition);
                        Console.WriteLine(_element.Name + ": potential low-priority XmlArray/XmlArrayItem list");
                    }
                }
                else
                {
                    Console.WriteLine(_element.Name + ": no potential XmlArray/XmlArrayItem list");
                }
            }
            else
            {
                Console.WriteLine(_element.Name + ": no potential XmlArray/XmlArrayItem list");
            }

            var listPropertyDefinitions = property.PotentialPropertyDefinitions
                .Select(x =>
                    new PropertyDefinition(ListClass.FromClass(x.Class), _pluralizationService.Value.Pluralize(_element.Name.ToString()))
                    {
                        Attributes = new List<AttributeProxy> { AttributeProxy.XmlElement(_element.Name.ToString()) }
                    }
                ).ToList();

            if (_element.Parent.Elements(_element.Name).Count() > 1)
            {
                property.PrependPotentialPropertyDefinitions(listPropertyDefinitions);
                Console.WriteLine(_element.Name + ": potential high-priority XmlElement list");
            }
            else
            {
                property.AppendPotentialPropertyDefinitions(listPropertyDefinitions);
                Console.WriteLine(_element.Name + ": potential low-priority XmlElement list");
            }

            Console.WriteLine(); Console.WriteLine();

            return property;
        }
    }
}

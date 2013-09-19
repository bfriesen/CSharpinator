using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CSharpifier
{
    public class PropertyDefinitionSetProxy
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int Order { get; set; }

        [XmlAttribute]
        public bool IsEnabled { get; set; }

        [XmlElement("PropertyDefinition")]
        public List<PropertyDefinitionProxy> PropertyDefinitions { get; set; }

        public static PropertyDefinitionSetProxy FromPropertyDefinitionSet(PropertyDefinitionSet propertyDefinitionSet)
        {
            return new PropertyDefinitionSetProxy
            {
                Name = propertyDefinitionSet.Name,
                Order = propertyDefinitionSet.Order,
                IsEnabled = propertyDefinitionSet.IsEnabled,
                PropertyDefinitions = propertyDefinitionSet.PropertyDefinitions.Select(PropertyDefinitionProxy.FromPropertyDefinition).ToList()
            };
        }

        public PropertyDefinitionSet ToPropertyDefinitionSet(IClassRepository classRepository, IFactory factory)
        {
            return new PropertyDefinitionSet
            {
                Name = Name,
                Order = Order,
                IsEnabled = IsEnabled,
                PropertyDefinitions = PropertyDefinitions.Select(x => x.ToPropertyDefinition(classRepository, factory)).ToList()
            };
        }
    }
}

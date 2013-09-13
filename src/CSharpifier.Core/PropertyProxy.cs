using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CSharpifier
{
    [XmlRoot("Property")]
    public class PropertyProxy
    {
        [XmlAttribute]
        public string Id { get; set; }
        [XmlAttribute]
        public bool HasHadNonEmptyValue { get; set; }
        [XmlElement("PropertyDefinition")]
        public List<PropertyDefinitionProxy> PotentialPropertyDefinitions { get; set; }

        public static PropertyProxy FromProperty(Property property)
        {
            return new PropertyProxy
            {
                Id = property.Id,
                HasHadNonEmptyValue = property.HasHadNonEmptyValue,
                PotentialPropertyDefinitions = property.PotentialPropertyDefinitions.Select(PropertyDefinitionProxy.FromPropertyDefinition).ToList()
            };
        }

        public Property ToProperty(IClassRepository classRepository)
        {
            var property = new Property(Id, HasHadNonEmptyValue);

            property.InitializePotentialPropertyDefinitions(
                propertyDefinitions =>
                propertyDefinitions.Append(PotentialPropertyDefinitions.Select(x => x.ToPropertyDefinition(classRepository))));
            
            return property;
        }
    }
}

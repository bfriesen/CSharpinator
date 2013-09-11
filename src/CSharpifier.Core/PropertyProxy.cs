using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CSharpifier
{
    [XmlRoot("Property")]
    public class PropertyProxy
    {
        public string Name { get; set; }
        public bool HasHadNonEmptyValue { get; set; }
        public List<PropertyDefinitionProxy> PotentialPropertyDefinitions { get; set; }

        public static PropertyProxy FromProperty(Property property)
        {
            return new PropertyProxy
            {
                Name = property.Name.Raw,
                HasHadNonEmptyValue = property.HasHadNonEmptyValue,
                PotentialPropertyDefinitions = property.PotentialPropertyDefinitions.Select(PropertyDefinitionProxy.FromPropertyDefinition).ToList()
            };
        }

        public Property ToProperty(IClassRepository classRepository)
        {
            var property = new Property(Name, HasHadNonEmptyValue);
            property.AppendPotentialPropertyDefinitions(PotentialPropertyDefinitions.Select(x => x.ToPropertyDefinition(classRepository)));
            return property;
        }
    }
}

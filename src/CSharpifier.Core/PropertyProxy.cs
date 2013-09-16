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

        [XmlElement("DefaultPropertyDefinitionSet")]
        public PropertyDefinitionSetProxy DefaultPropertyDefinitionSet { get; set; }

        [XmlArray("ExtraPropertyDefinitionSets")]
        [XmlArrayItem("PropertyDefinitionSet")]
        public List<PropertyDefinitionSetProxy> ExtraPropertyDefinitionSets { get; set; }

        public static PropertyProxy FromProperty(Property property)
        {
            return new PropertyProxy
            {
                Id = property.Id,
                HasHadNonEmptyValue = property.HasHadNonEmptyValue,
                DefaultPropertyDefinitionSet = PropertyDefinitionSetProxy.FromPropertyDefinitionSet(property.DefaultPropertyDefinitionSet),
                ExtraPropertyDefinitionSets = property.ExtraPropertyDefinitionSets.Select(PropertyDefinitionSetProxy.FromPropertyDefinitionSet).ToList()
            };
        }

        public Property ToProperty(IClassRepository classRepository)
        {
            var property = new Property(Id, HasHadNonEmptyValue);

            property.InitializeDefaultPropertyDefinitionSet(
                propertyDefinitions =>
                propertyDefinitions.Append(DefaultPropertyDefinitionSet.PropertyDefinitions.Select(x => x.ToPropertyDefinition(classRepository))));

            foreach (var set in ExtraPropertyDefinitionSets.Select(x => x.ToPropertyDefinitionSet(classRepository)))
            {
                property.AddOrUpdateExtraPropertyDefinitionSet(set);
            }
            
            return property;
        }
    }
}

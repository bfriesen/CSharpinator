using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CSharpinator
{
    [XmlRoot("Property")]
    public class PropertyProxy
    {
        [XmlAttribute]
        public string DomPath { get; set; }

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
                DomPath = string.Format("{0}:{1}", property.DomPath.FullPath, property.DomPath.TypeNameDepth),
                HasHadNonEmptyValue = property.HasHadNonEmptyValue,
                DefaultPropertyDefinitionSet = PropertyDefinitionSetProxy.FromPropertyDefinitionSet(property.DefaultPropertyDefinitionSet),
                ExtraPropertyDefinitionSets = property.ExtraPropertyDefinitionSets.Select(PropertyDefinitionSetProxy.FromPropertyDefinitionSet).ToList()
            };
        }

        public Property ToProperty(IClassRepository classRepository, IFactory factory)
        {
            var split = DomPath.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            var domPath = factory.GetOrCreateDomPath(split[0], int.Parse(split[1]));
            var property = factory.CreateProperty(domPath, HasHadNonEmptyValue);

            property.InitializeDefaultPropertyDefinitionSet(
                propertyDefinitions =>
                propertyDefinitions.Append(DefaultPropertyDefinitionSet.PropertyDefinitions.Select(x => x.ToPropertyDefinition(classRepository, factory))));

            foreach (var proxySet in ExtraPropertyDefinitionSets)
            {
                var set = proxySet.ToPropertyDefinitionSet(classRepository, factory);
                property.AddOrUpdateExtraPropertyDefinitionSet(set);
            }

            return property;
        }
    }
}

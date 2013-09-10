using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CSharpifier
{
    [XmlRoot("Property")]
    public class PropertyProxy
    {
        public string Name { get; set; }
        public List<PropertyDefinitionProxy> PotentialPropertyDefinitions { get; set; }

        public static PropertyProxy FromProperty(Property property)
        {
            return new PropertyProxy
            {
                Name = property.Name.Raw,
                PotentialPropertyDefinitions = property.PotentialPropertyDefinitions.Select(x => PropertyDefinitionProxy.FromPropertyDefinition(x)).ToList()
            };
        }

        public Property ToProperty(IClassRepository classRepository)
        {
            var property = new Property(XName.Get(Name));
            property.AppendPotentialPropertyDefinitions(PotentialPropertyDefinitions.Select(x => x.ToPropertyDefinition(classRepository)));
            return property;
        }
    }
}

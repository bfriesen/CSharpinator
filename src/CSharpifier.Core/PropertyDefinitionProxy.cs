using System.Collections.Generic;
using System.Xml.Serialization;

namespace CSharpifier
{
    [XmlRoot("PropertyDefinition")]
    public class PropertyDefinitionProxy
    {
        public string Name { get; set; }
        public ClassProxy Class { get; set; }
        public List<AttributeProxy> Attributes { get; set; }
        public bool IsLegal { get; set; }

        public static PropertyDefinitionProxy FromPropertyDefinition(PropertyDefinition propertyDefinition)
        {
            return new PropertyDefinitionProxy
            {
                Name = propertyDefinition.Name.Raw,
                Class = ClassProxy.FromClass(propertyDefinition.Class),
                Attributes = new List<AttributeProxy>(propertyDefinition.Attributes),
                IsLegal = propertyDefinition.IsLegal
            };
        }

        public PropertyDefinition ToPropertyDefinition(IClassRepository classRepository)
        {
            var @class = Class.ToClass(classRepository);
            var propertyDefinition = new PropertyDefinition(@class, Name)
            {
                Attributes = new List<AttributeProxy>(Attributes),
                IsLegal = IsLegal
            };
            return propertyDefinition;
        }
    }
}

using System.Collections.Generic;

namespace CSharpifier
{
    public class PropertyDefinitionSet
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public bool IsEnabled { get; set; }
        public List<PropertyDefinition> PropertyDefinitions { get; set; }
    }
}

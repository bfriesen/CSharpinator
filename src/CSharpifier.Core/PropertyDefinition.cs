using System.Collections.Generic;

namespace CSharpifier
{
    public class PropertyDefinition
    {
        public PropertyDefinition(Class @class, string propertyName, bool isLegal, bool isEnabled)
        {
            Attributes = new List<AttributeProxy>();
            Class = @class;
            Name = new IdentifierName(propertyName);
            IsLegal = isLegal;
            IsEnabled = isEnabled;
        }

        public Class Class { get; set; }
        public IdentifierName Name { get; set; }
        public List<AttributeProxy> Attributes { get; set; }
        public bool IsLegal { get; set; }
        public bool IsEnabled { get; set; }

        public string GeneratePropertyCode(Case classCase, Case propertyCase)
        {
            return string.Format("{0}", Class.GeneratePropertyCode(Name.FormatAs(propertyCase), classCase, Attributes));
        }
    }
}

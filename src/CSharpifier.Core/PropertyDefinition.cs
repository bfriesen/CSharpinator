using System.Collections.Generic;
using System.Text;

namespace CSharpifier
{
    public class PropertyDefinition
    {
        public PropertyDefinition(Class @class, string propertyName)
        {
            Class = @class;
            Name = new IdentifierName(propertyName);
            Attributes = new List<AttributeProxy>();
        }

        public Class Class { get; set; }
        public IdentifierName Name { get; set; }
        public List<AttributeProxy> Attributes { get; set; }
        public bool IsLegal { get; set; }

        public string GeneratePropertyCode(Case classCase, Case propertyCase)
        {
            var sb = new StringBuilder();

            foreach (var attribute in Attributes)
            {
                sb.AppendLine(string.Format("{0}", attribute.ToCode()));
            }

            return string.Format("{0}", Class.GeneratePropertyCode(Name.FormatAs(propertyCase), classCase, Attributes));

            return sb.ToString();
        }
    }
}

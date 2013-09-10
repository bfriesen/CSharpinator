using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CSharpifier
{
    public class PropertyDefinition
    {
        public PropertyDefinition(Class @class, XName propertyName)
        {
            Class = @class;
            Name = new IdentifierName(propertyName.ToString());
            Attributes = new List<AttributeProxy>();
        }

        public Class Class { get; set; }
        public IdentifierName Name { get; set; }
        public List<AttributeProxy> Attributes { get; set; }

        public string GeneratePropertyCode(Case classCase, Case propertyCase)
        {
            var sb = new StringBuilder();

            foreach (var attribute in Attributes)
            {
                sb.AppendLine(string.Format("        {0}", attribute.ToCode()));
            }

            sb.AppendFormat("        {0}", Class.GeneratePropertyCode(Name.FormatAs(propertyCase), classCase));

            return sb.ToString();
        }
    }
}

using System.Collections.Generic;
using System.Text;

namespace CSharpinator
{
    public class PascalCaseBooleanClass : BclClassBase
    {
        public PascalCaseBooleanClass()
            : base("PascalCaseBoolean", "bool", value => value == "True" || value == "False", value => value is bool)
        {
        }

        public override bool IsNullable
        {
            get { return false; }
        }

        public override string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes, DocumentType documentType)
        {
            var sb = new StringBuilder();

            var ignoreAttribute = documentType == DocumentType.Xml ? "[XmlIgnore]" : "[IgnoreDataMember]"; 

            sb.AppendFormat(
                @"{0}
public bool {1} {{ get; set; }}", ignoreAttribute, propertyName).AppendLine().AppendLine();

            foreach (var attribute in attributes)
            {
                sb.AppendLine(attribute.ToCode());
            }

            sb.AppendFormat(
                @"public string {0}String
{{
    get
    {{
        return {0} ? ""True"" : ""False"";
    }}
    set
    {{
        {0} = bool.Parse(value);
    }}
}}", propertyName);

            return sb.ToString();
        }
    }
}
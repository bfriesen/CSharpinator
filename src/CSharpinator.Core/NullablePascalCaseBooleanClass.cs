using System.Collections.Generic;
using System.Text;

namespace CSharpinator
{
    public class NullablePascalCaseBooleanClass : BclClassBase
    {
        public NullablePascalCaseBooleanClass()
            : base("NullablePascalCaseBoolean", "bool?", value => value == null || value == "True" || value == "False", value => value == null || value is bool)
        {
        }

        public override bool IsNullable { get { return true; } }

        public override string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes, DocumentType documentType)
        {
            var sb = new StringBuilder();

            var ignoreAttribute = documentType == DocumentType.Xml ? "[XmlIgnore]" : "[IgnoreDataMember]"; 

            sb.AppendFormat(
                @"{0}
public bool? {1} {{ get; set; }}", ignoreAttribute, propertyName).AppendLine().AppendLine();

            foreach (var attribute in attributes)
            {
                sb.AppendLine(attribute.ToCode());
            }

            sb.AppendFormat(
                @"public string {0}String
{{
    get
    {{
        return {0} == null ? null : {0}.Value ? ""True"" : ""False"";
    }}
    set
    {{
        {0} = value == null ? null : (bool?)bool.Parse(value);
    }}
}}", propertyName);

            return sb.ToString();
        }
    }
}
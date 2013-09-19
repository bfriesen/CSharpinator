using System.Collections.Generic;
using System.Text;

namespace CSharpifier
{
    public class NullablePascalCaseBooleanClass : BclClassBase
    {
        public NullablePascalCaseBooleanClass()
            : base("NullablePascalCaseBoolean", "bool?", value => value == null || value == "True" || value == "False")
        {
        }

        public override bool IsNullable { get { return true; } }

        public override string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes)
        {
            var sb = new StringBuilder();

            sb.AppendFormat(
                @"[XmlIgnore]
public bool? {0} {{ get; set; }}", propertyName).AppendLine().AppendLine();

            foreach (var attribute in attributes)
            {
                sb.AppendLine(string.Format("{0}", attribute.ToCode()));
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
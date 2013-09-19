using System.Collections.Generic;
using System.Text;

namespace CSharpifier
{
    public class PascalCaseBooleanClass : BclClassBase
    {
        public PascalCaseBooleanClass()
            : base("PascalCaseBoolean", "bool", value => value == "True" || value == "False")
        {
        }

        public override bool IsNullable
        {
            get { return false; }
        }

        public override string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes)
        {
            var sb = new StringBuilder();

            sb.AppendFormat(
                @"[XmlIgnore]
public bool {0} {{ get; set; }}", propertyName).AppendLine().AppendLine();

            foreach (var attribute in attributes)
            {
                sb.AppendLine(string.Format("{0}", attribute.ToCode()));
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
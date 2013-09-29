using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpinator
{
    public class NullableBclClass : BclClassBase
    {
        private readonly string _toString;
        private readonly string _parse;

        public NullableBclClass(
            string typeName,
            string typeAlias,
            Func<string, bool> isLegalStringValue,
            Func<object, bool> isLegalObjectValue,
            string toString,
            string parse)
            : base(typeName, typeAlias, isLegalStringValue, isLegalObjectValue)
        {
            _toString = toString;
            _parse = parse;
        }

        public override bool IsNullable { get { return true; } }

        public override string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes, DocumentType documentType)
        {
            if (documentType != DocumentType.Xml)
            {
                return base.GeneratePropertyCode(propertyName, classCase, attributes, documentType);
            }

            var sb = new StringBuilder();

            sb.AppendFormat(
                @"[XmlIgnore]
public {0} {1} {{ get; set; }}", TypeAlias, propertyName).AppendLine().AppendLine();

            foreach (var attribute in attributes)
            {
                sb.AppendLine(attribute.ToCode());
            }

            sb.AppendFormat(
                @"public string {0}String
{{
    get
    {{
        return {0} == null ? null : {0}{1};
    }}
    set
    {{
        {0} = string.IsNullOrEmpty(value) ? null : ({2}){3};
    }}
}}", propertyName, _toString, TypeAlias, _parse);

            return sb.ToString();
        }
    }
}
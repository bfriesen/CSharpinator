using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpinator
{
    public class NullableFormattedBoolean : FormattedBooleanBase
    {
        private static readonly ConcurrentDictionary<string, Func<string, bool>> _isLegalFuncs = new ConcurrentDictionary<string, Func<string, bool>>();

        public NullableFormattedBoolean(string formatString, IFactory factory)
            : base(formatString, GetIsLegalValueFunc(formatString))
        {
        }

        public override bool IsNullable
        {
            get { return true; }
        }

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

            var formats = GetFormats(FormatString);

//            sb.AppendFormat(
//@"public string {0}String
//{{
//    get
//    {{
//        if ({0} == null)
//        {{
//            return null;
//        }}
//
//{1}
//    }}
//    set
//    {{
//        if (value == null)
//        {{
//            {0} = null;
//        }}
//        else if (value)
//        {{
//            {0} = ""{2}"";
//        }}
//        else
//        {{
//            {0} = ""{3}"";
//        }}
//    }}
//}}",
//                propertyName,
//                GetNonNullableGetter(formats, propertyName),
//                formats.First().True,
//                formats.First().False);

            sb.AppendFormat(
@"public string {0}String
{{
    get
    {{
        if ({0}.HasValue)
        {{
            return {0}.Value ? ""{1}"" : ""{2}"";
        }}

        return null;
    }}
    set
    {{
        if (value == null)
        {{
            {0} = null;
            return;
        }}

{3}
    }}
}}",
                propertyName,
                formats.First().True,
                formats.First().False,
                GetNonNullableSetter(formats, propertyName));

            return sb.ToString();
        }

        protected override bool Equals(FormattedBooleanBase other)
        {
            var otherNullableFormattedBoolean = other as NullableFormattedBoolean;
            if (otherNullableFormattedBoolean == null)
            {
                return false;
            }

            return FormatString == otherNullableFormattedBoolean.FormatString;
        }

        private static Func<string, bool> GetIsLegalValueFunc(string formatString)
        {
            return _isLegalFuncs.GetOrAdd(
                formatString,
                f =>
                {
                    var formats = GetFormats(f);
                    return (Func<string, bool>)(value => formats.Any(format => value == format.True || value == format.False));
                });
        }
    }
}
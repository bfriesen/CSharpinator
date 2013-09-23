using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpinator
{
    public class FormattedBoolean : FormattedBooleanBase
    {
        private static readonly ConcurrentDictionary<string, Func<string, bool>> _isLegalFuncs = new ConcurrentDictionary<string, Func<string, bool>>();

        public FormattedBoolean(string formatString, IFactory factory)
            : base(formatString, GetIsLegalValueFunc(formatString))
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

            var formats = GetFormats(FormatString);

            sb.AppendFormat(
@"public string {0}String
{{
    get
    {{
        return {0} ? ""{1}"" : ""{2}"";
    }}
    set
    {{
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
            var otherFormattedBoolean = other as FormattedBoolean;
            if (otherFormattedBoolean == null)
            {
                return false;
            }

            return FormatString == otherFormattedBoolean.FormatString;
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
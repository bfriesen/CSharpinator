using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CSharpinator
{
    public class FormattedDateTime : BclClassBase
    {
        private readonly string _format;
        private readonly IFactory _factory;

        public FormattedDateTime(string format, IFactory factory)
            : base(
                "FormattedDateTime",
                "DateTime",
                value => { DateTime temp; return DateTime.TryParseExact(value, format, new CultureInfo("en-US"), DateTimeStyles.None, out temp); },
                value => value is DateTime)
        {
            _format = format;
            _factory = factory;
        }

        public override bool IsNullable
        {
            get { return false; }
        }

        public string Format
        {
            get { return _format; }
        }

        public override string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes, DocumentType documentType)
        {
            var sb = new StringBuilder();

            var ignoreAttribute = documentType == DocumentType.Xml ? "[XmlIgnore]" : "[IgnoreDataMember]"; 

            sb.AppendFormat(
                @"{0}
public DateTime {1} {{ get; set; }}", propertyName).AppendLine().AppendLine();

            foreach (var attribute in attributes)
            {
                sb.AppendLine(string.Format("{1}", attribute.ToCode()));
            }

            sb.AppendFormat(
                @"public string {1}String
{{
    get
    {{
        return {1}.ToString(""{2}"", new CultureInfo(""en-US""));
    }}
    set
    {{
        {1} = DateTime.ParseExact(value, ""{2}"", new CultureInfo(""en-US""));
    }}
}}", ignoreAttribute, propertyName, Format);

            return sb.ToString();
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (other.GetType() != GetType())
            {
                return false;
            }
            return Equals((FormattedDateTime)other);
        }

        protected bool Equals(FormattedDateTime other)
        {
            return base.Equals(other) && string.Equals(_format, other._format);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (_format != null ? _format.GetHashCode() : 0);
            }
        }
    }
}
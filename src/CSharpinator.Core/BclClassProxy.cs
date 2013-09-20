using System.Xml.Serialization;

namespace CSharpifier
{
    [XmlRoot("BclClass")]
    public class BclClassProxy : ClassProxy
    {
        [XmlAttribute]
        public string TypeAlias { get; set; }
        [XmlAttribute]
        public string TypeName { get; set; }

        public static BclClassProxy FromBclClass(IBclClass bclClass)
        {
            var formattedDateTime = bclClass as FormattedDateTime;
            if (formattedDateTime != null)
            {
                return new FormattedDateTimeProxy
                {
                    TypeAlias = formattedDateTime.TypeAlias,
                    TypeName = formattedDateTime.TypeName,
                    Format = formattedDateTime.Format
                };
            }

            var nullableFormattedDateTime = bclClass as NullableFormattedDateTime;
            if (nullableFormattedDateTime != null)
            {
                return new NullableFormattedDateTimeProxy
                {
                    TypeAlias = nullableFormattedDateTime.TypeAlias,
                    TypeName = nullableFormattedDateTime.TypeName,
                    Format = nullableFormattedDateTime.Format
                };
            }

            return new BclClassProxy
            {
                TypeAlias = bclClass.TypeAlias,
                TypeName = bclClass.TypeName
            };
        }

        public static IBclClass ToBclClass(BclClassProxy bclClassProxy, IFactory factory)
        {
            var formattedDateTime = bclClassProxy as FormattedDateTimeProxy;
            if (formattedDateTime != null)
            {
                return factory.GetOrCreateFormattedDateTime(formattedDateTime.Format);
            }

            var nullableFormattedDateTime = bclClassProxy as NullableFormattedDateTimeProxy;
            if (nullableFormattedDateTime != null)
            {
                return factory.GetOrCreateNullableFormattedDateTime(nullableFormattedDateTime.Format);
            }

            return factory.GetBclClassFromTypeName(bclClassProxy.TypeName);
        }
    }
}

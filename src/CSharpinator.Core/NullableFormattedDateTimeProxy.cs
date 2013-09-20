using System.Xml.Serialization;

namespace CSharpifier
{
    [XmlRoot("NullableFormattableDateTime")]
    public class NullableFormattedDateTimeProxy : BclClassProxy
    {
        [XmlAttribute]
        public string Format { get; set; }
    }
}
using System.Xml.Serialization;

namespace CSharpinator
{
    [XmlRoot("NullableFormattableDateTime")]
    public class NullableFormattedDateTimeProxy : BclClassProxy
    {
        [XmlAttribute]
        public string Format { get; set; }
    }
}
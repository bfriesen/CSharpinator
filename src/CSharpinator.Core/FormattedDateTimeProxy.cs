using System.Xml.Serialization;

namespace CSharpifier
{
    [XmlRoot("FormattableDateTime")]
    public class FormattedDateTimeProxy : BclClassProxy
    {
        [XmlAttribute]
        public string Format { get; set; }
    }
}
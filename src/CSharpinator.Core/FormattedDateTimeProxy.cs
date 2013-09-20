using System.Xml.Serialization;

namespace CSharpinator
{
    [XmlRoot("FormattableDateTime")]
    public class FormattedDateTimeProxy : BclClassProxy
    {
        [XmlAttribute]
        public string Format { get; set; }
    }
}
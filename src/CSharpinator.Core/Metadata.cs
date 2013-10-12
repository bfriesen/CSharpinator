using System.Collections.Generic;
using System.Xml.Serialization;

namespace CSharpinator
{
    public class Metadata
    {
        public Metadata()
        {
            DateTimeFormats = new HashSet<string>();
        }

        [XmlElement("DocumentType")]
        public DocumentType DocumentType { get; set; }

        [XmlArray("References")]
        [XmlArrayItem("Reference")]
        public HashSet<string> References { get; set; }

        [XmlArray("Usings")]
        [XmlArrayItem("Using")]
        public HashSet<string> Usings { get; set; }

        [XmlArray("DateTimeFormats")]
        [XmlArrayItem("DateTimeFormat")]
        public HashSet<string> DateTimeFormats { get; set; }

        [XmlArray("Classes")]
        [XmlArrayItem("Class")]
        public List<UserDefinedClassProxy> Classes { get; set; }
    }
}

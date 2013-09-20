using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CSharpinator
{
    public class ClassDefinitions
    {
        private const string SerializationSeparator = "~!%";

        public ClassDefinitions()
        {
            DateTimeFormats = new HashSet<string>();
        }

        [XmlIgnore]
        public HashSet<string> DateTimeFormats { get; set; }

        [XmlElement("Class")]
        public List<UserDefinedClassProxy> Classes { get; set; }

        [XmlAttribute("DateTimeFormats")]
        public string DateTimeFormatsString
        {
            get
            {
                if (DateTimeFormats == null || DateTimeFormats.Count == 0)
                {
                    return null;
                }

                return string.Join(SerializationSeparator, DateTimeFormats);
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                DateTimeFormats = new HashSet<string>(value.Split(new[] { SerializationSeparator }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public static ClassDefinitions FromClasses(IEnumerable<UserDefinedClass> classes)
        {
            var classDefinitions = new ClassDefinitions
            {
                Classes = classes.Select(UserDefinedClassProxy.FromUserDefinedClass).ToList()
            };
            return classDefinitions;
        }

        public void LoadToRepository(IClassRepository classRepository, IFactory factory)
        {
            ToClasses(classRepository, factory);
        }

        public IEnumerable<UserDefinedClass> ToClasses(IClassRepository classRepository, IFactory factory)
        {
            return Classes.Select(x => x.ToUserDefinedClass(classRepository, factory)).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CSharpinator
{
    [XmlRoot("UserDefinedClass")]
    public class UserDefinedClassProxy : ClassProxy
    {
        [XmlAttribute]
        public string DomPath { get; set; }

        [XmlElement("Property")]
        public List<PropertyProxy> Properties { get; set; }

        public DomPath GetDomPath(IFactory factory)
        {
            var split = DomPath.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            var fullPath = split[0];
            var typeNameDepth = int.Parse(split[1]);

            var domPath = factory.GetOrCreateDomPath(fullPath, typeNameDepth);
            return domPath;
        }

        public static UserDefinedClassProxy FromUserDefinedClass(UserDefinedClass userDefinedClass)
        {
            return new UserDefinedClassProxy
            {
                DomPath = string.Format("{0}:{1}", userDefinedClass.DomPath.FullPath, userDefinedClass.DomPath.TypeNameDepth),
                Properties = userDefinedClass.Properties.Select(PropertyProxy.FromProperty).ToList()
            };
        }
    }
}

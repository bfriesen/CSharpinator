using System;
using System.Xml.Serialization;

namespace CSharpifier
{
    [XmlRoot("BclClass")]
    public class BclClassProxy : ClassProxy
    {
        public string TypeAlias { get; set; }
        public string TypeName { get; set; }

        public static BclClassProxy FromBclClass(BclClass bclClass)
        {
            return new BclClassProxy
            {
                TypeAlias = bclClass.TypeAlias,
                TypeName = bclClass.TypeName
            };
        }

        public static BclClass ToBclClass(BclClassProxy bclClassProxy)
        {
            return BclClass.FromTypeFullName(bclClassProxy.TypeName);
        }
    }
}

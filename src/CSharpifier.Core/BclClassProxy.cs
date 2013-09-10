using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSharpifier
{
    [XmlRoot("BclClass")]
    public class BclClassProxy : ClassProxy
    {
        public string TypeName { get; set; }
        public string TypeFullName { get; set; }

        public static BclClassProxy FromBclClass(BclClass bclClass)
        {
            return new BclClassProxy
            {
                TypeName = bclClass.TypeName,
                TypeFullName = bclClass.Type.FullName
            };
        }

        public static BclClass ToBclClass(BclClassProxy bclClassProxy)
        {
            return BclClass.FromType(Type.GetType(bclClassProxy.TypeFullName));
        }
    }
}

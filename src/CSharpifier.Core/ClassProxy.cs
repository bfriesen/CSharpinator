using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSharpifier
{
    [XmlRoot("Class")]
    [XmlInclude(typeof(UserDefinedClassProxy))]
    [XmlInclude(typeof(BclClassProxy))]
    [XmlInclude(typeof(ListClassProxy))]
    public class ClassProxy
    {
        public static ClassProxy FromClass(Class @class)
        {
            if (@class is UserDefinedClass)
            {
                return UserDefinedClassProxy.FromUserDefinedClass((UserDefinedClass)@class);
            }
            else if (@class is BclClass)
            {
                return BclClassProxy.FromBclClass((BclClass)@class);
            }
            else
            {
                return ListClassProxy.FromListClass((ListClass)@class);
            }
        }

        public Class ToClass(IClassRepository classRepository)
        {
            if (this is UserDefinedClassProxy)
            {
                return classRepository.GetOrCreate(((UserDefinedClassProxy)this).TypeName);
            }
            else if (this is BclClassProxy)
            {
                return BclClassProxy.ToBclClass((BclClassProxy)this);
            }
            else
            {
                return ListClassProxy.ToListClass((ListClassProxy)this, classRepository);
            }
        }
    }
}

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
            var userDefinedClass = @class as UserDefinedClass;
            if (userDefinedClass != null)
            {
                return UserDefinedClassProxy.FromUserDefinedClass(userDefinedClass);
            }

            var bclClass = @class as BclClass;
            if (bclClass != null)
            {
                return BclClassProxy.FromBclClass(bclClass);
            }

            return ListClassProxy.FromListClass((ListClass)@class);
        }

        public Class ToClass(IClassRepository classRepository)
        {
            var userDefinedClassProxy = this as UserDefinedClassProxy;
            if (userDefinedClassProxy != null)
            {
                return classRepository.GetOrAdd(userDefinedClassProxy.TypeName);
            }

            var bclClassProxy = this as BclClassProxy;
            if (bclClassProxy != null)
            {
                return BclClassProxy.ToBclClass(bclClassProxy);
            }

            return ListClassProxy.ToListClass((ListClassProxy)this, classRepository);
        }
    }
}

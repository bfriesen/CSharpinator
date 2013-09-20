using System.Xml.Serialization;

namespace CSharpinator
{
    [XmlRoot("Class")]
    [XmlInclude(typeof(UserDefinedClassProxy))]
    [XmlInclude(typeof(BclClassProxy))]
    [XmlInclude(typeof(ListClassProxy))]
    public class ClassProxy
    {
        public static ClassProxy FromClass(IClass @class)
        {
            var userDefinedClass = @class as UserDefinedClass;
            if (userDefinedClass != null)
            {
                return UserDefinedClassProxy.FromUserDefinedClass(userDefinedClass);
            }

            var bclClass = @class as IBclClass;
            if (bclClass != null)
            {
                return BclClassProxy.FromBclClass(bclClass);
            }

            return ListClassProxy.FromListClass((ListClass)@class);
        }

        public IClass ToClass(IClassRepository classRepository, IFactory factory)
        {
            var userDefinedClassProxy = this as UserDefinedClassProxy;
            if (userDefinedClassProxy != null)
            {
                return classRepository.GetOrAdd(userDefinedClassProxy.GetDomPath(factory));
            }

            var bclClassProxy = this as BclClassProxy;
            if (bclClassProxy != null)
            {
                return BclClassProxy.ToBclClass(bclClassProxy, factory);
            }

            return ListClassProxy.ToListClass((ListClassProxy)this, classRepository, factory);
        }
    }
}

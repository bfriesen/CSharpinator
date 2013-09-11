using System.Collections.Generic;
using System.Linq;

namespace CSharpifier
{
    public class ClassDefinitions
    {
        public List<UserDefinedClassProxy> Classes { get; set; }

        public static ClassDefinitions FromClasses(IEnumerable<UserDefinedClass> classes)
        {
            var classDefinitions = new ClassDefinitions
            {
                Classes = classes.Select(UserDefinedClassProxy.FromUserDefinedClass).ToList()
            };
            return classDefinitions;
        }

        public IEnumerable<UserDefinedClass> ToClasses(IClassRepository classRepository)
        {
            return Classes.Select(x => x.ToUserDefinedClass(classRepository)).ToList();
        }
    }
}

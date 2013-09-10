using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpifier
{
    public class ClassDefinitions
    {
        public List<UserDefinedClassProxy> Classes { get; set; }

        public static ClassDefinitions FromClasses(IEnumerable<UserDefinedClass> classes)
        {
            var classDefinitions = new ClassDefinitions();
            classDefinitions.Classes =
                classes.Select(x => UserDefinedClassProxy.FromUserDefinedClass(x)).ToList();
            return classDefinitions;
        }

        public IEnumerable<UserDefinedClass> ToClasses(IClassRepository classRepository)
        {
            return Classes.Select(x => x.ToUserDefinedClass(classRepository)).ToList();
        }
    }
}

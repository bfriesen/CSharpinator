using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CSharpifier
{
    public class ClassDefinitions
    {
        [XmlElement("Class")]
        public List<UserDefinedClassProxy> Classes { get; set; }

        public static ClassDefinitions FromClasses(IEnumerable<UserDefinedClass> classes)
        {
            var classDefinitions = new ClassDefinitions
            {
                Classes = classes.Select(UserDefinedClassProxy.FromUserDefinedClass).ToList()
            };
            return classDefinitions;
        }

        public void LoadToRepository(IClassRepository classRepository)
        {
            ToClasses(classRepository);
        }

        public IEnumerable<UserDefinedClass> ToClasses(IClassRepository classRepository)
        {
            return Classes.Select(x => x.ToUserDefinedClass(classRepository)).ToList();
        }
    }
}

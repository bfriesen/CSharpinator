using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSharpifier
{
    public class ClassGenerator
    {
        private readonly IClassRepository _repository;

        public ClassGenerator(IClassRepository repository)
        {
            _repository = repository;
        }

        public void Write(Case classCase, Case propertyCase, PropertyAttributes propertyAttributes, TextWriter writer)
        {
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("using System.Xml.Serialization;");
            writer.WriteLine();
            writer.WriteLine("namespace YourNamespaceHere");
            writer.WriteLine("{");

            var stuff = GetUsedClasses(_repository.GetAll().First()).Distinct().ToList();

            writer.WriteLine(
                string.Join(
                    "\r\n\r\n",
                    GetUsedClasses(_repository.GetAll().First()).Distinct().Select(x => x.GenerateCSharpCode(classCase, propertyCase))));

            writer.WriteLine("}");
        }

        public IEnumerable<UserDefinedClass> GetUsedClasses(UserDefinedClass rootClass)
        {
            yield return rootClass;

            foreach (var childClass in rootClass.Properties
                .Select(x => x.PotentialPropertyDefinitions.First(y => y.IsLegal))
                .Where(x => IsUserDefinedClass(x.Class))
                .Select(x => GetUserDefinedClass(x.Class)))
            {
                yield return childClass;

                foreach (var grandchildClass in GetUsedClasses(childClass))
                {
                    yield return grandchildClass;
                }
            }
        }

        private static bool IsUserDefinedClass(Class @class)
        {
            if (@class is UserDefinedClass)
            {
                return true;
            }

            if (@class is BclClass)
            {
                return false;
            }

            return IsUserDefinedClass(((ListClass)@class).Class);
        }

        private static UserDefinedClass GetUserDefinedClass(Class @class)
        {
            if (@class is UserDefinedClass)
            {
                return (UserDefinedClass)@class;
            }

            return GetUserDefinedClass(((ListClass)@class).Class);
        }
    }
}

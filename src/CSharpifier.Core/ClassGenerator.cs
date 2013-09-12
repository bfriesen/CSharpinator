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
            writer.WriteLine("using System.Globalization;");
            writer.WriteLine("using System.Xml.Serialization;");
            writer.WriteLine();
            writer.WriteLine("namespace YourNamespaceHere");
            writer.WriteLine("{");

            writer.WriteLine(
                string.Join(
                    "\r\n\r\n",
                    _repository.GetUsedClasses().Select(x => x.GenerateCSharpCode(classCase, propertyCase))));

            writer.WriteLine("}");
        }
    }
}

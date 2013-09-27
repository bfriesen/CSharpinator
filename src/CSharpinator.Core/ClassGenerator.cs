using System.IO;
using System.Linq;

namespace CSharpinator
{
    public class ClassGenerator
    {
        private readonly IClassRepository _repository;

        public ClassGenerator(IClassRepository repository)
        {
            _repository = repository;
        }

        public void Write(string @namespace, Case classCase, Case propertyCase, TextWriter writer, bool skipNamespace, DocumentType documentType)
        {
            if (!skipNamespace)
            {
                WriteUsings(writer, documentType);
                writer.WriteLine();
                writer.WriteLine("namespace {0}", @namespace);
                writer.WriteLine("{");
            }

            var usedClasses = _repository.GetUsedClasses().ToList();

            if (usedClasses.Count == 0)
            {
                writer.WriteLine("/* No used classes found! */");
            }

            writer.WriteLine(
                string.Join(
                    "\r\n\r\n",
                    _repository.GetUsedClasses().Select(x => x.GenerateCSharpCode(classCase, propertyCase, documentType))));

            if (!skipNamespace)
            {
                writer.WriteLine("}");
            }
        }

        private void WriteUsings(TextWriter writer, DocumentType documentType)
        {
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("using System.Globalization;");

            if (documentType == DocumentType.Xml)
            {
                writer.WriteLine("using System.Xml.Serialization;");
            }
            else if (documentType == DocumentType.Json)
            {
                writer.WriteLine("using System.Runtime.Serialization;");
            }
        }
    }
}

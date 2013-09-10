using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CSharpifier
{
    class Program
    {
        static void Main(string[] args)
        {
            XDocument xDocument;

            //if (args == null)
            {
                xDocument = XDocument.Parse(@"
<foo>
  <bars>
    <bar id=""1"">abc</bar>
    <bar id=""1"">xyz</bar>
    <baz>true</baz>
  </bars>
</foo>");
            }
            //else
            //{
            //    if (args.Length == 0)
            //    {
            //        Console.WriteLine("Must include path to document as first argument.");
            //        return;
            //    }

            //    if (!File.Exists(args[0]))
            //    {
            //        Console.WriteLine("No file exists at: " + args[0]);
            //        return;
            //    }

            //    try
            //    {
            //        xDocument = XDocument.Load(args[0]);
            //    }
            //    catch
            //    {
            //        Console.WriteLine("Error reading into XDocument for file: " + args[0]);
            //        return;
            //    }
            //}

            //xDocument.Root.ToString().Dump(); "".Dump();

            var domElement = new XmlDomElement(xDocument.Root);

            var classRepository = new ClassRepository();

            var domVisitor = new DomVisitor(classRepository);
            domVisitor.Visit(domElement);

            //    var classes = classRepository.GetAll();
            //    classes.Dump();
            //
            //    var classDefinitions = ClassDefinitions.FromClasses(classes);
            //    classDefinitions.Dump();
            //
            //    var loadedClasses = classDefinitions.ToClasses(classRepository);
            //    loadedClasses.Dump();

            var classGenerator = new ClassGenerator(classRepository);
            classGenerator.Write(
                Case.PascalCase,
                Case.PascalCase,
                PropertyAttributes.XmlSerializion | PropertyAttributes.DataContract,
                Console.Out);

            Console.Write("Press any key to exit..."); Console.ReadKey(true);
        }
    }
}

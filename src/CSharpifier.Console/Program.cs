using System;
using System.IO;
using System.Xml.Linq;

namespace CSharpifier
{
    class Program
    {
        static void Main(string[] args)
        {
            XDocument xDocument;

            if (args.Length == 0)
            {
                Console.WriteLine("Must include path to document as first argument.");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("No file exists at: " + args[0]);
                return;
            }

            try
            {
                xDocument = XDocument.Load(args[0]);
            }
            catch
            {
                Console.WriteLine("Error reading into XDocument for file: " + args[0]);
                return;
            }

//            xDocument = XDocument.Parse(@"
//            <foo>
//              <bars>
//                <bar>123</bar>
//                <bar>2147483648</bar>
//              </bars>
//            </foo>");

            var domElement = new XmlDomElement(xDocument.Root);

            var classRepository = new ClassRepository();

            var domVisitor = new DomVisitor(classRepository);
            domVisitor.Visit(domElement);

            var classGenerator = new ClassGenerator(classRepository);
            classGenerator.Write(
                Case.PascalCase,
                Case.PascalCase,
                PropertyAttributes.XmlSerializion | PropertyAttributes.DataContract,
                Console.Out);
        }
    }
}

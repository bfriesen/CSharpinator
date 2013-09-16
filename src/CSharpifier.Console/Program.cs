using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using Mono.Options;

namespace CSharpifier
{
    class Program
    {
        static void Main(string[] args)
        {
            bool showHelp = false;
            string @namespace = "YOUR_NAMESPACE";
            string output = null;
            string meta = null;
            string classCaseString = "PascalCase";
            string propertyCaseString = "PascalCase";

            var p = new OptionSet
            {
                { "n|namespace=", "The {NAMESPACE} to use for the generated classes.", value => @namespace = value },
                { "o|output=", "The path for the output file. If not provided, output will be written to standard out.", value => output = value },
                { "m|meta=", "The path to the metadata file used to describe the classes. If not provided, no metadata will be saved.", value => meta = value },
                { "c|class_case=", "The casing to be used for class names. Valid values are 'PascalCase', 'camelCase', and 'snake_case'. Default is 'PascalCase'.", value => classCaseString = value },
                { "p|property_case=", "The casing to be used for property names. Valid values are 'PascalCase', 'camelCase', and 'snake_case'. Default is 'PascalCase'.", value => propertyCaseString = value },
                { "h|help", "Show this message and exit.", value => showHelp = value != null },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("csharpify: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `csharpify --help' for more information.");
                return;
            }

            if (showHelp)
            {
                ShowHelp(p);
                return;
            }

            if (extra.Count != 1)
            {
                Console.WriteLine("Error: no document provided.");
                Console.WriteLine();
                ShowHelp(p);
                return;
            }

            Case classCase;
            if (!Enum.TryParse(classCaseString, out classCase))
            {
                Console.WriteLine("Error: invalid value for class_case: " + classCaseString);
                Console.WriteLine();
                ShowHelp(p);
                return;
            }

            Case propertyCase;
            if (!Enum.TryParse(propertyCaseString, out propertyCase))
            {
                Console.WriteLine("Error: invalid value for property_case: " + propertyCaseString);
                Console.WriteLine();
                ShowHelp(p);
                return;
            }

            TextWriter outWriter = output == null ? Console.Out : new StreamWriter(output, false);

            IDomElement domElement;
            try
            {
                domElement = GetRootElement(extra[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to create valid document: " + ex.Message);
                return;
            }

            var classRepository = new ClassRepository();

            var serializer = new XmlSerializer(typeof(ClassDefinitions));

            var metaExists = meta != null && File.Exists(meta);
            if (metaExists)
            {
                using (var reader = new StreamReader(meta))
                {
                    ((ClassDefinitions)serializer.Deserialize(reader)).LoadToRepository(classRepository);
                }
            }
            
            var domVisitor = new DomVisitor(classRepository);

            try
            {
                domVisitor.Visit(domElement, metaExists);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to process document: " + ex.Message);
                return;
            }

            if (meta != null)
            {
                try
                {
                    var classDefinitions = ClassDefinitions.FromClasses(classRepository.GetAll());

                    string tempFileName;
                    using (var writer = new StreamWriter(tempFileName = Path.GetTempFileName()))
                    {
                        serializer.Serialize(writer, classDefinitions);
                    }

                    var metaContents = File.ReadAllText(tempFileName);

                    using (var writer = new StreamWriter(meta, false))
                    {
                        writer.Write(metaContents);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error writing metadata file: " + ex.Message);
                }
            }

            var classGenerator = new ClassGenerator(classRepository);

            try
            {
                classGenerator.Write(
                    @namespace,
                    classCase,
                    propertyCase,
                    outWriter);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                if (outWriter is StreamWriter)
                {
                    outWriter.Close();
                }
            }
        }

        private static IDomElement GetRootElement(string arg)
        {
            IDomElement domElement;

            if (!File.Exists(arg))
            {
                if (!TryParseDocument(arg, out domElement))
                {
                    throw new InvalidArgumentsException("No file exists at: " + arg);
                }
            }
            else
            {
                if (!TryLoadDocument(arg, out domElement))
                {
                    throw new InvalidArgumentsException("Error reading into XDocument for file: " + arg);
                }
            }

            return domElement;
        }

        private static bool TryParseDocument(string arg, out IDomElement domElement)
        {
            try
            {
                var xDocument = XDocument.Parse(arg);
                domElement = new XmlDomElement(xDocument.Root);
                return true;
            }
            catch
            {
                domElement = null;
                return false;
            }
        }

        private static bool TryLoadDocument(string arg, out IDomElement domElement)
        {
            try
            {
                var xDocument = XDocument.Load(arg);
                domElement = new XmlDomElement(xDocument.Root);
                return true;
            }
            catch
            {
                domElement = null;
                return false;
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: csharpify DOCUMENT [OPTIONS]+");
            Console.WriteLine();
            Console.WriteLine("Generates a set of c# classes based on an xml or json document.");
            Console.WriteLine("DOCUMENT can be the path to an xml or json document, or the document itself.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CSharpinator
{
    class Program
    {
        static void Main(string[] args)
        {
            bool showHelp = false;
            string @namespace = "YOUR_NAMESPACE";
            bool skipNamespace = false;
            string output = null;
            string meta = null;
            string classCaseString = "PascalCase";
            string propertyCaseString = "PascalCase";
            var dateTimeFormats = new HashSet<string>();

            var p = new OptionSet
            {
                { "n|namespace=", "The {NAMESPACE} to use for the generated classes.", value => @namespace = value },
                { "s|skip_namespace", "Whether to skip writing out the namespace declaration and using statements.", value => skipNamespace = value != null },
                { "o|output=", "The path for the output file. If not provided, output will be written to standard out.", value => output = value },
                { "m|meta=", "The path to the metadata file used to describe the classes. If not provided, no metadata will be saved.", value => meta = value },
                { "c|class_case=", "The casing to be used for class names. Valid values are 'PascalCase', 'camelCase', and 'snake_case'. Default is 'PascalCase'.", value => classCaseString = value },
                { "p|property_case=", "The casing to be used for property names. Valid values are 'PascalCase', 'camelCase', and 'snake_case'. Default is 'PascalCase'.", value => propertyCaseString = value },
                { "d|date_time_format=", "A custom date time format string used for the serialization of date time fields.", value => dateTimeFormats.Add(value) },
                { "h|help", "Show this message and exit.", value => showHelp = value != null },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("csharpinate: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `csharpinate --help' for more information.");
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

            var configuration = new Configuration { JsonRootElementName = "RootElement" }; // TODO: get json root element name from command line, if provided.
            foreach (var dateTimeFormat in dateTimeFormats)
            {
                configuration.DateTimeFormats.Add(dateTimeFormat);
            }

            var factory = new Factory(configuration);
            var classRepository = new ClassRepository();
            var serializer = new XmlSerializer(typeof(ClassDefinitions));

            var metaExists = meta != null && File.Exists(meta);
            if (metaExists)
            {
                using (var reader = new StreamReader(meta))
                {
                    var classDefinitions = (ClassDefinitions)serializer.Deserialize(reader);

                    foreach (var dateTimeFormat in classDefinitions.DateTimeFormats)
                    {
                        configuration.DateTimeFormats.Add(dateTimeFormat);
                    }

                    classDefinitions.LoadToRepository(classRepository, factory);
                }
            }

            var outWriter = output == null ? Console.Out : new StreamWriter(output, false);

            IDomElement domElement;
            try
            {
                domElement = GetRootElement(extra[0], factory);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to create valid document: " + ex.Message);
                return;
            }

            var domVisitor = new DomVisitor(classRepository, factory);

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
                    foreach (var dateTimeFormat in configuration.DateTimeFormats)
                    {
                        classDefinitions.DateTimeFormats.Add(dateTimeFormat);
                    }

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
                    outWriter,
                    skipNamespace);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error!");
                Console.WriteLine(ex);
            }
            finally
            {
                if (outWriter is StreamWriter)
                {
                    outWriter.Close();
                }
            }
        }

        private static IDomElement GetRootElement(string arg, IFactory factory)
        {
            IDomElement domElement;

            if (!File.Exists(arg))
            {
                if (!TryParseDocument(arg, factory, out domElement))
                {
                    throw new InvalidArgumentsException("No file exists at: " + arg);
                }
            }
            else
            {
                if (!TryLoadDocument(arg, factory, out domElement))
                {
                    throw new InvalidArgumentsException("Error reading into XDocument for file: " + arg);
                }
            }

            return domElement;
        }

        private static bool TryParseDocument(string arg, IFactory factory, out IDomElement domElement)
        {
            try
            {
                var xDocument = XDocument.Parse(arg);
                domElement = factory.CreateXmlDomElement(xDocument.Root);
                return true;
            }
            catch
            {
                try
                {
                    var jToken = JToken.Parse(arg);
                    domElement = factory.CreateJsonDomElement(jToken);
                    return true;
                }
                catch
                {
                    domElement = null;
                    return false;
                }
            }
        }

        private static bool TryLoadDocument(string arg, IFactory factory, out IDomElement domElement)
        {
            try
            {
                var xDocument = XDocument.Load(arg);
                domElement = factory.CreateXmlDomElement(xDocument.Root);
                return true;
            }
            catch
            {
                try
                {
                    JToken jToken;
                    using (var streamReader = new StreamReader(arg))
                    {
                        using (var jsonReader = new JsonTextReader(streamReader))
                        {
                            jToken = JToken.Load(jsonReader);
                        }
                    }

                    domElement = factory.CreateJsonDomElement(jToken);
                    return true;
                }
                catch
                {
                    domElement = null;
                    return false;
                }
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: csharpinate DOCUMENT [OPTIONS]+");
            Console.WriteLine();
            Console.WriteLine("Generates a set of c# classes based on an xml or json document.");
            Console.WriteLine("DOCUMENT can be the path to an xml or json document, or the document itself.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}

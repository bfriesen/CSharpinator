using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CSharpinator
{
    class Program
    {
        [STAThread]
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
            string jsonRootElement = "RootElement";

            var p = new OptionSet
            {
                { "n|namespace=", "The {NAMESPACE} to use for the generated classes.", value => @namespace = value },
                { "s|skip_namespace", "Whether to skip writing out the namespace declaration and using statements.", value => skipNamespace = value != null },
                { "o|output=", "Where the output should be written to. If the value passed in is \"clipboard\", then the classes will be copied to the system clipboard. If the value provided is a file path, the classes will be written to that path. If no value is provided, output will be written to console.", value => output = value },
                { "m|meta=", "The path to the metadata file used to describe the classes. If not provided, no metadata will be saved.", value => meta = value },
                { "c|class_case=", "The casing to be used for class names. Valid values are PascalCase, camelCase, and snake_case. Default is PascalCase.", value => classCaseString = value },
                { "p|property_case=", "The casing to be used for property names. Valid values are PascalCase, camelCase, and snake_case. Default is PascalCase.", value => propertyCaseString = value },
                { "d|date_time_format=", "A custom date time format string used for the serialization of date time fields.", value => dateTimeFormats.Add(value) },
                { "r|root_element=", "When processing a JSON document, specifies the name of the class that corresponds with the top-level JSON object.", value => jsonRootElement = value },
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

            if (extra.Count != 1 && string.IsNullOrEmpty(meta))
            {
                Console.WriteLine("Error: no document or metadata provided.");
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

            var repository = new Repository(dateTimeFormats, jsonRootElement);

            var factory = new Factory(repository);
            var serializer = new XmlSerializer(typeof(Metadata));

            var metaExists = meta != null && File.Exists(meta);
            if (metaExists)
            {
                using (var reader = new StreamReader(meta))
                {
                    var metadata = (Metadata)serializer.Deserialize(reader);
                    repository.LoadFromMetadata(metadata, factory);
                }
            }

            TextWriter outWriter;
            if (output == null)
            {
                outWriter = Console.Out;
            }
            else if (output == "clipboard")
            {
                outWriter = new ClipboardWriter();
            }
            else
            {
                outWriter = new StreamWriter(output, false);
            }

            var documentType = DocumentType.Invalid;

            if (extra.Count == 1)
            {
                IDomElement domElement;
                try
                {
                    domElement = GetRootElement(extra[0], factory, out documentType);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to create valid document: " + ex.Message);
                    return;
                }

                repository.SetDocumentType(documentType);

                var domVisitor = new DomVisitor(repository, factory);

                try
                {
                    domVisitor.Visit(domElement, metaExists);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to process document: " + ex.Message);
                    return;
                }
            }
                
            if (meta != null)
            {
                try
                {
                    var metadata = repository.CreateMetadata();

                    documentType = metadata.DocumentType;

                    string tempFileName;
                    using (var writer = new StreamWriter(tempFileName = Path.GetTempFileName()))
                    {
                        serializer.Serialize(writer, metadata);
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

            var classGenerator = new ClassGenerator(repository);

            try
            {
                classGenerator.Write(
                    @namespace,
                    classCase,
                    propertyCase,
                    outWriter,
                    skipNamespace,
                    documentType);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error!");
                Console.WriteLine(ex);
            }
            finally
            {
                outWriter.Close();
            }
        }

        private static IDomElement GetRootElement(string arg, IFactory factory, out DocumentType documentType)
        {
            IDomElement domElement;

            if (!File.Exists(arg))
            {
                if (!TryParseDocument(arg, factory, out domElement, out documentType))
                {
                    throw new InvalidArgumentsException("No file exists at: " + arg);
                }
            }
            else
            {
                if (!TryLoadDocument(arg, factory, out domElement, out documentType))
                {
                    throw new InvalidArgumentsException("Error reading into XDocument for file: " + arg);
                }
            }

            return domElement;
        }

        private static bool TryParseDocument(string arg, IFactory factory, out IDomElement domElement, out DocumentType documentType)
        {
            try
            {
                var xDocument = XDocument.Parse(arg);
                domElement = factory.CreateXmlDomElement(xDocument.Root);
                documentType = DocumentType.Xml;
                return true;
            }
            catch
            {
                try
                {
                    var jToken = JToken.Parse(arg);
                    domElement = factory.CreateJsonDomElement(jToken);
                    documentType = DocumentType.Json;
                    return true;
                }
                catch
                {
                    domElement = null;
                    documentType = DocumentType.Invalid;
                    return false;
                }
            }
        }

        private static bool TryLoadDocument(string arg, IFactory factory, out IDomElement domElement, out DocumentType documentType)
        {
            try
            {
                var xDocument = XDocument.Load(arg);
                domElement = factory.CreateXmlDomElement(xDocument.Root);
                documentType = DocumentType.Xml;
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
                    documentType = DocumentType.Json;
                    return true;
                }
                catch
                {
                    domElement = null;
                    documentType = DocumentType.Invalid;
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

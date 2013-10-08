using Microsoft.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CSharpinator.Core.Tests
{
    public abstract class CompileTestBase
    {
        private static readonly string[] DefaultAssemblies = { "System.dll", "System.Core.dll", "System.Data.dll", "System.Xml.dll", "System.Xml.Linq.dll", "System.Runtime.Serialization.dll" };

        public const string JsonRootElementName = "RootElement";
        public const string Namespace = "DefaultNamespace";

        private static readonly JsonSerializer _jsonSerializer = JsonSerializer.Create();

        private Lazy<IFactory> _factory;
        private Lazy<IRepository> _repository;
        private Lazy<DomVisitor> _visitor;
        private Lazy<ClassGenerator> _classGenerator;

        private IDomElement _rootElement;
        private DocumentType _documentType;
        private IList<CompilerError> _compileErrors;

        public static string JsonRootElementFullName { get { return Namespace + "." + JsonRootElementName; } }

        public static JsonSerializer Serializer { get { return _jsonSerializer; } }

        public IFactory Factory { get { return _factory.Value; } }
        public IRepository Repository { get { return _repository.Value; } }
        public DomVisitor Visitor { get { return _visitor.Value; } }
        public ClassGenerator ClassGenerator { get { return _classGenerator.Value; } }

        public IDomElement RootElement { get { return _rootElement; } }
        public DocumentType DocumentType { get { return _documentType; } }
        public IEnumerable<CompilerError> CompileErrors { get { return _compileErrors; } }

        [SetUp]
        public void Setup()
        {
            _repository = new Lazy<IRepository>(() => new Repository { JsonRootElementName = JsonRootElementName });
            _factory = new Lazy<IFactory>(() => new Factory(_repository.Value));

            _visitor = new Lazy<DomVisitor>(() => new DomVisitor(Repository, Factory));
            _classGenerator = new Lazy<ClassGenerator>(() => new ClassGenerator(Repository));
        }

        protected string StripWhitespace(string document)
        {
            return Regex.Replace(document, @"\s", "");
        }

        protected string SerializeToJson(object instance)
        {
            var writer = new StringWriter();
            _jsonSerializer.Serialize(writer, instance);
            return writer.GetStringBuilder().ToString();
        }

        protected dynamic CreateFromJson(string jsonDocument)
        {
            var sourceCode = GetSourceCode(jsonDocument);
            var instance = CreateFromJson(jsonDocument, sourceCode);

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.GetType().FullName, Is.EqualTo(JsonRootElementFullName));
            Assert.That(SerializeToJson(instance), Is.EqualTo(jsonDocument));

            return instance;
        }
        
        private dynamic CreateFromJson(string jsonDocument, string sourceCode)
        {
            var compileErrors = new List<CompilerError>();
            CSharpCodeProvider provider = new CSharpCodeProvider();

            CompilerParameters parameters = new CompilerParameters();

            parameters.TreatWarningsAsErrors = false;
            parameters.ReferencedAssemblies.AddRange(DefaultAssemblies);
            parameters.GenerateInMemory = true;

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, sourceCode);

            if (results.Errors.Count > 0)
            {
                foreach (CompilerError error in results.Errors)
                {
                    compileErrors.Add(error);
                }
            }

            if (compileErrors.Count > 0)
            {
                Console.WriteLine(string.Join(Environment.NewLine, compileErrors));
                throw new CompileException(string.Join(Environment.NewLine, compileErrors));
            }

            dynamic instance;
            try
            {
                var type = results.CompiledAssembly.GetType(JsonRootElementFullName);
                instance = _jsonSerializer.Deserialize(new StringReader(jsonDocument), type);
            }
            catch (Exception ex)
            {
                throw new CompileException("Error creating instance with type name: " + JsonRootElementFullName, ex);
            }

            _compileErrors = compileErrors;

            return instance;
        }

        private string GetSourceCode(string document)
        {
            SetRootElement(document);
            Visitor.Visit(_rootElement, false);
            var writer = new StringWriter();
            ClassGenerator.Write(Namespace, Case.PascalCase, Case.PascalCase, writer, false, _documentType);
            return writer.GetStringBuilder().ToString();
        }

        private void SetRootElement(string document)
        {
            if (!ParseDocument(document, Factory, out _rootElement, out _documentType))
            {
                throw new InvalidOperationException("Unable to create root element from document.");
            }
        }

        private static bool ParseDocument(string document, IFactory factory, out IDomElement rootElement, out DocumentType documentType)
        {
            try
            {
                var xDocument = XDocument.Parse(document);
                rootElement = factory.CreateXmlDomElement(xDocument.Root);
                documentType = DocumentType.Xml;
                return true;
            }
            catch
            {
                try
                {
                    var jToken = JToken.Parse(document);
                    rootElement = factory.CreateJsonDomElement(jToken);
                    documentType = DocumentType.Json;
                    return true;
                }
                catch
                {
                    rootElement = null;
                    documentType = DocumentType.Invalid;
                    return false;
                }
            }
        }

        public class CompileException : Exception
        {
            public CompileException(string message)
                : base(message)
            {
            }

            public CompileException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }
    }
}

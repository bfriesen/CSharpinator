using Microsoft.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CSharpinator.Core.Tests
{
    public abstract class CompileTestBase
    {
        private static readonly string[] DefaultAssemblies = { "System.dll", "System.Core.dll", "System.Data.dll", "System.Xml.dll", "System.Xml.Linq.dll", "System.Runtime.Serialization.dll" };

        protected const string _rootElementName = "RootElement";

        protected static readonly JsonSerializer _jsonSerializer = JsonSerializer.Create();

        protected IFactory _factory;
        protected IClassRepository _repository;
        protected DomVisitor _visitor;
        protected ClassGenerator _classGenerator;

        protected IDomElement _rootElement;
        protected DocumentType _documentType;

        [SetUp]
        public void Setup()
        {
            var configuration = new Configuration { JsonRootElementName = _rootElementName };

            _factory = new Factory(configuration);
            _repository = new ClassRepository();

            _visitor = new DomVisitor(_repository, _factory);
            _classGenerator = new ClassGenerator(_repository);
        }

        protected string SerializeJson(object instance)
        {
            var writer = new StringWriter();
            _jsonSerializer.Serialize(writer, instance);
            return writer.GetStringBuilder().ToString();
        }

        protected dynamic CreateObjectFromJson(string jsonDocument)
        {
            var sourceCode = GetSourceCode(jsonDocument);

            var typeName = "DefaultNamespace." + _rootElementName;

            CompilerErrorCollection compileErrors;

            compileErrors = new CompilerErrorCollection();
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
                Console.WriteLine(string.Join(Environment.NewLine, compileErrors.Cast<CompilerError>()));
                throw new CompileException(string.Join(Environment.NewLine, compileErrors));
            }

            dynamic instance;
            try
            {
                var type = results.CompiledAssembly.GetType(typeName);
                instance = _jsonSerializer.Deserialize(new StringReader(jsonDocument), type);
            }
            catch (Exception ex)
            {
                throw new CompileException("Error creating instance with type name: " + typeName, ex);
            }

            return instance;
        }

        private string GetSourceCode(string document)
        {
            SetRootElement(document);
            _visitor.Visit(_rootElement, false);
            var writer = new StringWriter();
            _classGenerator.Write("DefaultNamespace", Case.PascalCase, Case.PascalCase, writer, false, _documentType);
            return writer.GetStringBuilder().ToString();
        }

        private void SetRootElement(string document)
        {
            if (!ParseDocument(document, _factory, out _rootElement, out _documentType))
            {
                throw new InvalidOperationException("Unable to create root element from document.");
            }
        }

        private static bool ParseDocument(string document, IFactory factory, out IDomElement domElement, out DocumentType documentType)
        {
            try
            {
                var xDocument = XDocument.Parse(document);
                domElement = factory.CreateXmlDomElement(xDocument.Root);
                documentType = DocumentType.Xml;
                return true;
            }
            catch
            {
                try
                {
                    var jToken = JToken.Parse(document);
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

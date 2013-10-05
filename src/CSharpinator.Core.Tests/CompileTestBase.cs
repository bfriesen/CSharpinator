using Microsoft.CSharp;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CSharpinator.Core.Tests
{
    public abstract class CompileTestBase
    {
        private static readonly string[] DefaultAssemblies = { "System.dll", "System.Core.dll", "System.Data.dll", "System.Xml.dll", "System.Xml.Linq.dll" };

        protected static dynamic CreateObject(string sourceCode, string typeName, params string[] referencedAssemblies)
        {
            CompilerErrorCollection compileErrors;

            compileErrors = new CompilerErrorCollection();
            CSharpCodeProvider provider = new CSharpCodeProvider();

            CompilerParameters parameters = new CompilerParameters();

            parameters.TreatWarningsAsErrors = false;
            parameters.ReferencedAssemblies.AddRange(DefaultAssemblies);
            parameters.ReferencedAssemblies.AddRange(referencedAssemblies);
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
                throw new CompileException(string.Join(Environment.NewLine, compileErrors));
            }

            dynamic instance;
            try
            {
                instance = results.CompiledAssembly.CreateInstance(typeName);
            }
            catch (Exception ex)
            {
                throw new CompileException("Error creating instance with type name: " + typeName, ex);
            }

            return instance;
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

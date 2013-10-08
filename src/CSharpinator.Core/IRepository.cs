using System.Collections.Generic;

namespace CSharpinator
{
    public interface IRepository
    {
        IEnumerable<UserDefinedClass> GetAll();
        UserDefinedClass GetOrAdd(DomPath path);
        UserDefinedClass GetOrAdd(DomPath path, out bool isNew);

        void LoadFromMetadata(Metadata metadata, IFactory factory);
        Metadata CreateMetadata();

        IEnumerable<string> References { get; }
        IEnumerable<string> Usings { get; }
        IEnumerable<string> DateTimeFormats { get; }

        string JsonRootElementName { get; }
    }
}

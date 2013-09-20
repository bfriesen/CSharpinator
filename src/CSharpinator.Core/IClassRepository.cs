using System.Collections.Generic;

namespace CSharpifier
{
    public interface IClassRepository
    {
        IEnumerable<UserDefinedClass> GetAll();
        UserDefinedClass GetOrAdd(DomPath path);
        UserDefinedClass GetOrAdd(DomPath path, out bool isNew);
    }
}

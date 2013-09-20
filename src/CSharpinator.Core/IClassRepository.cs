using System.Collections.Generic;

namespace CSharpinator
{
    public interface IClassRepository
    {
        IEnumerable<UserDefinedClass> GetAll();
        UserDefinedClass GetOrAdd(DomPath path);
        UserDefinedClass GetOrAdd(DomPath path, out bool isNew);
    }
}

using System.Collections.Generic;

namespace CSharpifier
{
    public interface IClassRepository
    {
        IEnumerable<UserDefinedClass> GetAll();
        UserDefinedClass GetOrAdd(string typeName);
        UserDefinedClass GetOrAdd(string typeName, out bool isNew);
    }
}

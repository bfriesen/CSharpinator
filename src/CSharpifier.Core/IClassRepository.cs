using System.Collections.Generic;

namespace CSharpifier
{
    public interface IClassRepository
    {
        IEnumerable<UserDefinedClass> GetAll();
        UserDefinedClass GetOrCreate(string typeName);
        UserDefinedClass GetOrCreate(string typeName, out bool isNew);
    }
}

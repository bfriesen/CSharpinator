using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CSharpifier
{
    public class ClassRepository : IClassRepository
    {
        private static readonly ConcurrentDictionary<string, UserDefinedClass> _classes
            = new ConcurrentDictionary<string, UserDefinedClass>();

        public IEnumerable<UserDefinedClass> GetAll()
        {
            return _classes.Values.OrderBy(x => x.Order);
        }

        public UserDefinedClass GetOrCreate(string typeName)
        {
            return _classes.GetOrAdd(
                typeName,
                x => new UserDefinedClass(typeName));
        }
    }
}

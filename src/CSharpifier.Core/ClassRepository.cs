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

        public UserDefinedClass GetOrAdd(string typeName)
        {
            return _classes.GetOrAdd(
                typeName,
                x => new UserDefinedClass(typeName));
        }

        public UserDefinedClass GetOrAdd(string typeName, out bool isNew)
        {
            var isNewClass = false;
            var @class = _classes.GetOrAdd(
                typeName,
                x =>
                {
                    isNewClass = true;
                    return new UserDefinedClass(typeName);
                });
            isNew = isNewClass;
            return @class;
        }
    }
}

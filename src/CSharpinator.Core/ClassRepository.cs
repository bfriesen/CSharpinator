using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CSharpinator
{
    public class ClassRepository : IClassRepository
    {
        private readonly ConcurrentDictionary<DomPath, UserDefinedClass> _classes = new ConcurrentDictionary<DomPath, UserDefinedClass>();

        public IEnumerable<UserDefinedClass> GetAll()
        {
            return _classes.Values.OrderBy(x => x.Order);
        }

        public UserDefinedClass GetOrAdd(DomPath path)
        {
            return _classes.GetOrAdd(
                path,
                x => new UserDefinedClass(x));
        }

        public UserDefinedClass GetOrAdd(DomPath path, out bool isNew)
        {
            var isNewClass = false;
            var @class = _classes.GetOrAdd(
                path,
                x =>
                {
                    isNewClass = true;
                    return new UserDefinedClass(x);
                });
            isNew = isNewClass;
            return @class;
        }
    }
}

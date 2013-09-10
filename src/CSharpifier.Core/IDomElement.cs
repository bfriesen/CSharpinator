using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpifier
{
    public interface IDomElement
    {
        bool HasElements { get; }
        string Value { get; }
        string Name { get; }
        IEnumerable<IDomElement> Elements { get; }
        Property CreateProperty(IClassRepository classRepository);
    }
}

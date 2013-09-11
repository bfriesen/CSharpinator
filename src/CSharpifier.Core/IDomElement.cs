using System.Collections.Generic;

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

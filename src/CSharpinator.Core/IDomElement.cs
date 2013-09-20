using System.Collections.Generic;

namespace CSharpinator
{
    public interface IDomElement
    {
        bool HasElements { get; }
        string Value { get; }
        string Name { get; }
        IEnumerable<IDomElement> Elements { get; }
        Property CreateProperty(IClassRepository classRepository);
        DomPath GetDomPath(IFactory factory);
    }
}

using System.Collections.Generic;

namespace CSharpifier
{
    public interface IClass
    {
        string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes);
    }
}

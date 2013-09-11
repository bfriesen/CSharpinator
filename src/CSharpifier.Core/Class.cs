using System.Collections.Generic;

namespace CSharpifier
{
    public abstract class Class
    {
        public abstract string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes);
    }
}

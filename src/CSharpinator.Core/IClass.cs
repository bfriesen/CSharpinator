using System.Collections.Generic;

namespace CSharpinator
{
    public interface IClass
    {
        string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes, DocumentType documentType);
    }
}

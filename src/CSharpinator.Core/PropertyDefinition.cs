using System.Collections.Generic;

namespace CSharpinator
{
    public class PropertyDefinition
    {
        private readonly IFactory _factory;

        public PropertyDefinition(IClass @class, string propertyName, bool isLegal, bool isEnabled, IFactory factory)
        {
            Attributes = new List<AttributeProxy>();
            Class = @class;
            Name = new IdentifierName(propertyName);
            IsLegal = isLegal;
            IsEnabled = isEnabled;
            _factory = factory;
        }

        public IClass Class { get; set; }
        public IdentifierName Name { get; set; }
        public List<AttributeProxy> Attributes { get; set; }
        public bool IsLegal { get; set; }
        public bool IsEnabled { get; set; }

        public string GeneratePropertyCode(Case classCase, Case propertyCase)
        {
            return Class.GeneratePropertyCode(Name.FormatAs(propertyCase), classCase, Attributes);
        }

        public override string ToString()
        {
            return
                (Class is IBclClass
                     ? ((IBclClass)Class).TypeAlias
                     : Class is UserDefinedClass
                           ? ((UserDefinedClass)Class).TypeName.Raw
                           : "List<" +
                             (((ListClass)Class).Class is IBclClass
                                  ? ((IBclClass)((ListClass)Class).Class).TypeAlias
                                  : ((UserDefinedClass)((ListClass)Class).Class).TypeName.Raw)
                             + ">")
                + ":"
                + (IsLegal ? "Legal" : "Illegal")
                + ":"
                + (IsEnabled ? "Enabled" : "Disabled");
        }
    }
}

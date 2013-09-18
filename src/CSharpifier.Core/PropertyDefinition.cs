using System.Collections.Generic;

namespace CSharpifier
{
    public class PropertyDefinition
    {
        public PropertyDefinition(IClass @class, string propertyName, bool isLegal, bool isEnabled)
        {
            Attributes = new List<AttributeProxy>();
            Class = @class;
            Name = new IdentifierName(propertyName);
            IsLegal = isLegal;
            IsEnabled = isEnabled;
        }

        public IClass Class { get; set; }
        public IdentifierName Name { get; set; }
        public List<AttributeProxy> Attributes { get; set; }
        public bool IsLegal { get; set; }
        public bool IsEnabled { get; set; }

        public string GeneratePropertyCode(Case classCase, Case propertyCase)
        {
            return string.Format("{0}", Class.GeneratePropertyCode(Name.FormatAs(propertyCase), classCase, Attributes));
        }

        public override string ToString()
        {
            return
                (Class is BclClass
                     ? ((BclClass)Class).TypeAlias
                     : Class is UserDefinedClass
                           ? ((UserDefinedClass)Class).TypeName.Raw
                           : "List<" +
                             (((ListClass)Class).Class is BclClass
                                  ? ((BclClass)((ListClass)Class).Class).TypeAlias
                                  : ((UserDefinedClass)((ListClass)Class).Class).TypeName.Raw)
                             + ">")
                + ":"
                + (IsLegal ? "Legal" : "Illegal")
                + ":"
                + (IsEnabled ? "Enabled" : "Disabled");
        }
    }
}

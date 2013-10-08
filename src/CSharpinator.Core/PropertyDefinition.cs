﻿using System.Collections.Generic;

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

        public string GeneratePropertyCode(Case classCase, Case propertyCase, DocumentType documentType, string customPropertyName)
        {
            var propertyName =
                string.IsNullOrEmpty(customPropertyName)
                ? Name.FormatAs(propertyCase, Class.IsPlural)
                : customPropertyName;

            return Class.GeneratePropertyCode(propertyName, classCase, Attributes, documentType);
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

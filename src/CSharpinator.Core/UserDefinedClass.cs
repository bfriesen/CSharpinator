using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CSharpinator
{
    [DebuggerDisplay("{TypeName.Raw}")]
    public class UserDefinedClass : IClass
    {
        private static int _orderSeed;

        private readonly Dictionary<DomPath, Property> _properties = new Dictionary<DomPath, Property>();
        private readonly DomPath _domPath;
        private readonly int _order;

        public UserDefinedClass(DomPath domPath)
        {
            _domPath = domPath;
            _order = _orderSeed++;
        }

        public IdentifierName TypeName { get { return DomPath.TypeName; } }
        public int Order { get { return _order; } }
        public IEnumerable<Property> Properties { get { return _properties.Values; } }
        public DomPath DomPath { get { return _domPath; } }

        public void AddProperty(Property property, bool isParentClassNew, bool metaExists)
        {
            Property foundProperty;
            if (!_properties.TryGetValue(property.DomPath, out foundProperty))
            {
                // If we're adding a new property to an old class, it should be nullable.
                if (!isParentClassNew && metaExists)
                {
                    property.MakeNullable();
                }

                _properties.Add(property.DomPath, property);
                return;
            }

            foundProperty.MergeWith(property);
        }

        public string GenerateCSharpCode(Case classCase, Case propertyCase, DocumentType documentType)
        {
            return string.Format(
    @"{0}
public partial class {1}
{{
{2}
}}",
                GetClassAttribute(documentType),
                DomPath.TypeName.FormatAs(classCase),
                string.Join("\r\n\r\n", Properties.Select(x => x.GeneratePropertyCode(classCase, propertyCase).Indent())))
                .Indent();
        }

        private string GetClassAttribute(DocumentType documentType)
        {
            if (documentType == DocumentType.Xml)
            {
                return string.Format(@"[XmlRoot(""{0}"")]", DomPath.ElementName.Raw);
            }

            return "[DataContract]";
        }

        public virtual string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes)
        {
            var sb = new StringBuilder();

            foreach (var attribute in attributes)
            {
                sb.AppendLine(string.Format("{0}", attribute.ToCode()));
            }

            sb.AppendFormat("public {0} {1} {{ get; set; }}", DomPath.TypeName.FormatAs(classCase), propertyName);

            return sb.ToString();
        }

        public override bool Equals(object other)
        {
            var otherUserDefinedClass = other as UserDefinedClass;
            if (otherUserDefinedClass == null)
            {
                return false;
            }

            return DomPath.Equals(otherUserDefinedClass.DomPath);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ("UserDefinedClass".GetHashCode() * 397) ^ DomPath.GetHashCode();
            }
        }
    }
}

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CSharpifier
{
    [DebuggerDisplay("{TypeName.Raw}")]
    public class UserDefinedClass : Class
    {
        private static int _orderSeed;

        private readonly Dictionary<string, Property> _properties = new Dictionary<string, Property>();
        private readonly IdentifierName _typeName;
        private readonly int _order;

        public UserDefinedClass(string typeName)
        {
            _typeName = new IdentifierName(typeName);
            _order = _orderSeed++;
        }

        public IdentifierName TypeName
        {
            get { return _typeName; }
        }

        public int Order
        {
            get { return _order; }
        }

        public IEnumerable<Property> Properties
        {
            get { return _properties.Values; }
        }

        public void AddProperty(Property property, bool isParentClassNew, bool metaExists)
        {
            Property foundProperty;
            if (!_properties.TryGetValue(property.Id, out foundProperty))
            {
                // If we're adding a new property to an old class, it should be nullable.
                if (!isParentClassNew && metaExists)
                {
                    property.MakeNullable();
                }

                _properties.Add(property.Id, property);
                return;
            }

            foundProperty.MergeWith(property);
        }

        public string GenerateCSharpCode(Case classCase, Case propertyCase)
        {
            return string.Format(
    @"[XmlRoot(""{0}"")]
public partial class {1}
{{
{2}
}}",
                _typeName.Raw,
                _typeName.FormatAs(classCase),
                string.Join("\r\n\r\n", Properties.Select(x => x.GeneratePropertyCode(classCase, propertyCase).Indent())))
                .Indent();
        }

        public override string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes)
        {
            var sb = new StringBuilder();

            foreach (var attribute in attributes)
            {
                sb.AppendLine(string.Format("{0}", attribute.ToCode()));
            }

            sb.AppendFormat("public {0} {1} {{ get; set; }}", _typeName.FormatAs(classCase), propertyName);

            return sb.ToString();
        }

        public override bool Equals(object other)
        {
            var otherUserDefinedClass = other as UserDefinedClass;
            if (otherUserDefinedClass == null)
            {
                return false;
            }

            return _typeName.Raw == otherUserDefinedClass._typeName.Raw;
        }

        public override int GetHashCode()
        {
            return _typeName.Raw.GetHashCode();
        }
    }
}

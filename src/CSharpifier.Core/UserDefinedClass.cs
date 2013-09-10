using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpifier
{
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

        public void AddProperty(Property property)
        {
            Property foundProperty;
            if (!_properties.TryGetValue(property.Name.Raw, out foundProperty))
            {
                _properties.Add(property.Name.Raw, property);
                return;
            }

            foundProperty.AppendPotentialPropertyDefinitions(property.PotentialPropertyDefinitions);
        }

        public UserDefinedClass MergeWith(UserDefinedClass other)
        {
            if (TypeName != other.TypeName)
            {
                // We shouldn't hit this situation, but just in case, bail.
                return this;
            }

            foreach (var otherProperty in other.Properties)
            {
                AddProperty(otherProperty);
            }

            return this;
        }

        public string GenerateCSharpCode(Case classCase, Case propertyCase)
        {
            return string.Format(
    @"    [XmlRoot(""{0}"")]
    public partial class {1}
    {{
{2}
    }}",
                _typeName.Raw,
                _typeName.FormatAs(classCase),
                string.Join("\r\n\r\n", Properties.Select(x => x.GeneratePropertyCode(classCase, propertyCase))));
        }

        public override string GeneratePropertyCode(string propertyName, Case classCase)
        {
            return string.Format("public {0} {1} {{ get; set; }}", _typeName.FormatAs(classCase), propertyName);
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

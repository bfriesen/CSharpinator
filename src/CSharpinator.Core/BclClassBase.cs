using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpinator
{
    public abstract class BclClassBase : IBclClass
    {
        private readonly string _typeName;
        private readonly string _typeAlias;
        private readonly Func<string, bool> _isLegalStringValue;
        private readonly Func<object, bool> _isLegalObjectValue;

        protected BclClassBase(string typeName, string typeAlias, Func<string, bool> isLegalStringValue, Func<object, bool> isLegalObjectValue)
        {
            _typeName = typeName;
            _typeAlias = typeAlias;
            _isLegalStringValue = isLegalStringValue;
            _isLegalObjectValue = isLegalObjectValue;
        }

        public string TypeName { get { return _typeName; } }

        public string TypeAlias { get { return _typeAlias; } }

        public bool IsLegalStringValue(string value)
        {
            return _isLegalStringValue(value);
        }

        public bool IsLegalObjectValue(object value)
        {
            var valueAsString = value as string;
            if (valueAsString != null)
            {
                return _isLegalStringValue(valueAsString) || _isLegalObjectValue(valueAsString);
            }

            return _isLegalObjectValue(value);
        }

        public abstract bool IsNullable { get; }

        public bool IsPlural { get { return false; } }

        public virtual string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes, DocumentType documentType)
        {
            var sb = new StringBuilder();

            foreach (var attribute in attributes)
            {
                sb.AppendLine(attribute.ToCode());
            }

            sb.AppendFormat("public {0} {1} {{ get; set; }}", TypeAlias, propertyName);

            return sb.ToString();
        }

        public override bool Equals(object other)
        {
            var otherBclClass = other as IBclClass;
            if (otherBclClass == null)
            {
                return false;
            }

            return _typeName == otherBclClass.TypeName;
        }

        public override int GetHashCode()
        {
            return _typeName.GetHashCode();
        }
    }
}
using System;
using System.Collections.Generic;

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

        public abstract string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes);

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
using System;
using System.Collections.Generic;

namespace CSharpifier
{
    public abstract class BclClassBase : IBclClass
    {
        private readonly string _typeName;
        private readonly string _typeAlias;
        private readonly Func<string, bool> _isLegalValue;

        protected BclClassBase(string typeName, string typeAlias, Func<string, bool> isLegalValue)
        {
            _typeName = typeName;
            _typeAlias = typeAlias;
            _isLegalValue = isLegalValue;
        }

        public string TypeName { get { return _typeName; } }

        public string TypeAlias { get { return _typeAlias; } }

        public bool IsLegalValue(string value)
        {
            return _isLegalValue(value);
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
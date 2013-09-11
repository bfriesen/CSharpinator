using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CSharpifier
{
    public class BclClass : Class
    {
        private static readonly ConcurrentDictionary<Type, BclClass> _classes = new ConcurrentDictionary<Type, BclClass>();
        private readonly Type _type;
        private readonly string _typeName;
        private readonly Func<string, bool> _isLegalValue;

        private BclClass(Type type, string typeName, Func<string, bool> isLegalValue)
        {
            _type = type;
            _typeName = typeName;
            _isLegalValue = isLegalValue;
        }

        public override string GeneratePropertyCode(string propertyName, Case classCase)
        {
            return string.Format("public {0} {1} {{ get; set; }}", _typeName, propertyName);
        }

        public override bool Equals(object other)
        {
            var otherBclClass = other as BclClass;
            if (otherBclClass == null)
            {
                return false;
            }

            return _type == otherBclClass._type;
        }

        public override int GetHashCode()
        {
            return _type.GetHashCode();
        }

        public Type Type { get { return _type; } }
        public string TypeName { get { return _typeName; } }

        public bool IsLegalValue(string value)
        {
            return _isLegalValue(value);
        }

        public static BclClass String
        {
            get { return _classes.GetOrAdd(typeof(string), type => new BclClass(type, "string", value => true)); }
        }

        public static BclClass Boolean
        {
            get { return _classes.GetOrAdd(typeof(bool), type => new BclClass(type, "bool", value => { bool temp; return bool.TryParse(value, out temp); })); }
        }

        public static BclClass Int32
        {
            get { return _classes.GetOrAdd(typeof(int), type => new BclClass(type, "int", value => { int temp; return int.TryParse(value, out temp); })); }
        }

        public static BclClass Int64
        {
            get { return _classes.GetOrAdd(typeof(long), type => new BclClass(type, "long", value => { long temp; return long.TryParse(value, out temp); })); }
        }

        public static BclClass Decimal
        {
            get { return _classes.GetOrAdd(typeof(decimal), type => new BclClass(type, "decimal", value => { decimal temp; return decimal.TryParse(value, out temp); })); }
        }

        public static BclClass DateTime
        {
            get { return _classes.GetOrAdd(typeof(DateTime), type => new BclClass(type, "DateTime", value => { DateTime temp; return System.DateTime.TryParse(value, out temp); })); }
        }

        public static BclClass Guid
        {
            get { return _classes.GetOrAdd(typeof(Guid), type => new BclClass(type, "Guid", value => { Guid temp; return System.Guid.TryParse(value, out temp); })); }
        }

        public static BclClass FromType(Type type)
        {
            if (type == typeof(string))
            {
                return String;
            }
            if (type == typeof(bool))
            {
                return Boolean;
            }
            if (type == typeof(int))
            {
                return Int32;
            }
            if (type == typeof(long))
            {
                return Int64;
            }
            if (type == typeof(decimal))
            {
                return Decimal;
            }
            if (type == typeof(DateTime))
            {
                return DateTime;
            }
            if (type == typeof(Guid))
            {
                return Guid;
            }

            throw new InvalidOperationException("Invalid type for BclClass: " + type);
        }

        public static IEnumerable<BclClass> All
        {
            get
            {
                yield return Int32;
                yield return Int64;
                yield return Decimal;
                yield return Boolean;
                yield return Guid;
                yield return DateTime;
                yield return String;
            }
        }
    }
}

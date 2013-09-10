using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpifier
{
    public class BclClass : Class
    {
        private static readonly ConcurrentDictionary<Type, BclClass> _classes = new ConcurrentDictionary<Type, BclClass>();
        private readonly Type _type;
        private readonly string _typeName;

        private BclClass(Type type, string typeName)
        {
            _type = type;
            _typeName = typeName;
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

        public static BclClass String
        {
            get { return _classes.GetOrAdd(typeof(string), type => new BclClass(type, "string")); }
        }

        public static BclClass Boolean
        {
            get { return _classes.GetOrAdd(typeof(bool), type => new BclClass(type, "bool")); }
        }

        public static BclClass Int32
        {
            get { return _classes.GetOrAdd(typeof(int), type => new BclClass(type, "int")); }
        }

        public static BclClass Decimal
        {
            get { return _classes.GetOrAdd(typeof(decimal), type => new BclClass(type, "decimal")); }
        }

        public static BclClass DateTime
        {
            get { return _classes.GetOrAdd(typeof(DateTime), type => new BclClass(type, "DateTime")); }
        }

        public static BclClass Guid
        {
            get { return _classes.GetOrAdd(typeof(Guid), type => new BclClass(type, "Guid")); }
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

        public static IEnumerable<BclClass> GetLegalClassesFromValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                yield return String;
                yield break;
            }

            int tempInt;
            if (int.TryParse(value, out tempInt))
            {
                yield return Int32;
            }

            decimal tempDecimal;
            if (decimal.TryParse(value, out tempDecimal))
            {
                yield return Decimal;
            }

            bool tempBool;
            if (bool.TryParse(value, out tempBool))
            {
                yield return Boolean;
            }

            System.Guid tempGuid;
            if (System.Guid.TryParse(value, out tempGuid))
            {
                yield return Guid;
            }

            System.DateTime tempDateTime;
            if (System.DateTime.TryParse(value, out tempDateTime))
            {
                yield return DateTime;
            }

            yield return String;
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CSharpifier
{
    public class BclClass : Class
    {
        private static readonly ConcurrentDictionary<string, BclClass> _classes = new ConcurrentDictionary<string, BclClass>();
        private readonly string _typeName;
        private readonly string _typeAlias;
        private readonly Func<string, bool> _isLegalValue;

        private BclClass(string typeName, string typeAlias, Func<string, bool> isLegalValue)
        {
            _typeName = typeName;
            _typeAlias = typeAlias;
            _isLegalValue = isLegalValue;
        }

        public override string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes)
        {
            var sb = new StringBuilder();

            foreach (var attribute in attributes)
            {
                sb.AppendLine(string.Format("{0}", attribute.ToCode()));
            }

            sb.AppendFormat("public {0} {1} {{ get; set; }}", _typeAlias, propertyName);

            return sb.ToString();
        }

        public override bool Equals(object other)
        {
            var otherBclClass = other as BclClass;
            if (otherBclClass == null)
            {
                return false;
            }

            return _typeName == otherBclClass._typeName;
        }

        public override int GetHashCode()
        {
            return _typeName.GetHashCode();
        }

        public string TypeName { get { return _typeName; } }
        public string TypeAlias { get { return _typeAlias; } }

        public bool IsLegalValue(string value)
        {
            return _isLegalValue(value);
        }

        public static BclClass String
        {
            get { return _classes.GetOrAdd(typeof(string).FullName, typeName => new BclClass(typeName, "string", value => true)); }
        }

        public static BclClass Boolean
        {
            get { return _classes.GetOrAdd(typeof(bool).FullName, typeName => new BclClass(typeName, "bool", value => value == "true" || value == "false")); }
        }

        public static BclClass PascalCaseBoolean
        {
            get { return _classes.GetOrAdd("PascalCaseBoolean", typeName => new PascalCaseBooleanClass()); }
        }

        public static BclClass Int32
        {
            get { return _classes.GetOrAdd(typeof(int).FullName, typeName => new BclClass(typeName, "int", value => { int temp; return int.TryParse(value, out temp); })); }
        }

        public static BclClass Int64
        {
            get { return _classes.GetOrAdd(typeof(long).FullName, typeName => new BclClass(typeName, "long", value => { long temp; return long.TryParse(value, out temp); })); }
        }

        public static BclClass Decimal
        {
            get { return _classes.GetOrAdd(typeof(decimal).FullName, typeName => new BclClass(typeName, "decimal", value => { decimal temp; return decimal.TryParse(value, out temp); })); }
        }

        public static BclClass DateTime
        {
            get { return _classes.GetOrAdd(typeof(DateTime).FullName, typeName => new BclClass(typeName, "DateTime", value => { DateTime temp; return System.DateTime.TryParse(value, out temp); })); }
        }

        public static BclClass Guid
        {
            get { return _classes.GetOrAdd(typeof(Guid).FullName, typeName => new BclClass(typeName, "Guid", value => { Guid temp; return System.Guid.TryParse(value, out temp); })); }
        }

        public static BclClass FromTypeFullName(string typeFullName)
        {
            if (typeFullName == "PascalCaseBoolean")
            {
                return PascalCaseBoolean;
            }

            var type = Type.GetType(typeFullName);

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
                yield return PascalCaseBoolean;
                yield return Guid;
                yield return DateTime;
                yield return String;
            }
        }

        private class PascalCaseBooleanClass : BclClass
        {
            public PascalCaseBooleanClass()
                : base("PascalCaseBoolean", "bool", value => value.ToLower() == "true" || value.ToLower() == "false")
            {
            }

            public override string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes)
            {
                var sb = new StringBuilder();

                sb.AppendFormat(
@"[XmlIgnore]
public bool {0} {{ get; set; }}", propertyName).AppendLine().AppendLine();


                foreach (var attribute in attributes)
                {
                    sb.AppendLine(string.Format("{0}", attribute.ToCode()));
                }

                sb.AppendFormat(
@"public string {0}Raw
{{
    get
    {{
        return {0} ? ""True"" : ""False"";
    }}
    set
    {{
        {0} = bool.Parse(value);
    }}
}}", propertyName);

                return sb.ToString();
            }
        }
    }
}

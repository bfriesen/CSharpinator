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
        public virtual bool IsNullable { get { return false; } }

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

        public static BclClass NullableBoolean
        {
            get { return _classes.GetOrAdd(typeof(bool?).FullName, typeName => new NullableBclClass(typeName, "bool", value => string.IsNullOrEmpty(value) || value == "true" || value == "false", ".ToString()", "bool.Parse(value)")); }
        }

        public static BclClass PascalCaseBoolean
        {
            get { return _classes.GetOrAdd("PascalCaseBoolean", typeName => new PascalCaseBooleanClass()); }
        }

        public static BclClass NullablePascalCaseBoolean
        {
            get { return _classes.GetOrAdd("NullablePascalCaseBoolean", typeName => new NullablePascalCaseBooleanClass()); }
        }

        public static BclClass Int32
        {
            get { return _classes.GetOrAdd(typeof(int).FullName, typeName => new BclClass(typeName, "int", value => { int temp; return int.TryParse(value, out temp); })); }
        }

        public static BclClass NullableInt32
        {
            get { return _classes.GetOrAdd(typeof(int?).FullName, typeName => new NullableBclClass(typeName, "int", value => { int temp; return string.IsNullOrEmpty(value) || int.TryParse(value, out temp); }, ".ToString()", "int.Parse(value)")); }
        }

        public static BclClass Int64
        {
            get { return _classes.GetOrAdd(typeof(long).FullName, typeName => new BclClass(typeName, "long", value => { long temp; return long.TryParse(value, out temp); })); }
        }

        public static BclClass NullableInt64
        {
            get { return _classes.GetOrAdd(typeof(long?).FullName, typeName => new NullableBclClass(typeName, "long", value => { long temp; return string.IsNullOrEmpty(value) || long.TryParse(value, out temp); }, ".ToString()", "long.Parse(value)")); }
        }

        public static BclClass Decimal
        {
            get { return _classes.GetOrAdd(typeof(decimal).FullName, typeName => new BclClass(typeName, "decimal", value => { decimal temp; return decimal.TryParse(value, out temp); })); }
        }

        public static BclClass NullableDecimal
        {
            get { return _classes.GetOrAdd(typeof(decimal?).FullName, typeName => new NullableBclClass(typeName, "decimal", value => { decimal temp; return string.IsNullOrEmpty(value) || decimal.TryParse(value, out temp); }, ".ToString()", "decimal.Parse(value)")); }
        }

        public static BclClass DateTime
        {
            get { return _classes.GetOrAdd(typeof(DateTime).FullName, typeName => new BclClass(typeName, "DateTime", value => { DateTime temp; return System.DateTime.TryParse(value, out temp); })); }
        }

        public static BclClass NullableDateTime
        {
            get { return _classes.GetOrAdd(typeof(DateTime?).FullName, typeName => new NullableBclClass(typeName, "DateTime", value => { DateTime temp; return string.IsNullOrEmpty(value) || System.DateTime.TryParse(value, out temp); }, ".ToString()", @"DateTime.ParseExact(value, ""o"", CultureInfo.CurrentCulture);")); }
        }

        public static BclClass Guid
        {
            get { return _classes.GetOrAdd(typeof(Guid).FullName, typeName => new BclClass(typeName, "Guid", value => { Guid temp; return System.Guid.TryParse(value, out temp); })); }
        }

        public static BclClass NullableGuid
        {
            get { return _classes.GetOrAdd(typeof(Guid?).FullName, typeName => new NullableBclClass(typeName, "Guid", value => { Guid temp; return string.IsNullOrEmpty(value) || System.Guid.TryParse(value, out temp); }, ".ToString()", "Guid.Parse(value)")); }
        }

        public static BclClass FromTypeFullName(string typeFullName)
        {
            if (typeFullName == "PascalCaseBoolean")
            {
                return PascalCaseBoolean;
            }

            if (typeFullName == "NullablePascalCaseBoolean")
            {
                return NullablePascalCaseBoolean;
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
            if (type == typeof(bool?))
            {
                return NullableBoolean;
            }
            if (type == typeof(int))
            {
                return Int32;
            }
            if (type == typeof(int?))
            {
                return NullableInt32;
            }
            if (type == typeof(long))
            {
                return Int64;
            }
            if (type == typeof(long?))
            {
                return NullableInt64;
            }
            if (type == typeof(decimal))
            {
                return Decimal;
            }
            if (type == typeof(decimal?))
            {
                return NullableDecimal;
            }
            if (type == typeof(DateTime))
            {
                return DateTime;
            }
            if (type == typeof(DateTime?))
            {
                return NullableDateTime;
            }
            if (type == typeof(Guid))
            {
                return Guid;
            }
            if (type == typeof(Guid?))
            {
                return NullableGuid;
            }

            throw new InvalidOperationException("Invalid type for BclClass: " + type);
        }

        public static IEnumerable<BclClass> All
        {
            get
            {
                yield return Int32;
                yield return NullableInt32;
                yield return Int64;
                yield return NullableInt64;
                yield return Decimal;
                yield return NullableDecimal;
                yield return Boolean;
                yield return NullableBoolean;
                yield return PascalCaseBoolean;
                yield return NullablePascalCaseBoolean;
                yield return Guid;
                yield return NullableGuid;
                yield return DateTime;
                yield return NullableDateTime;
                yield return String;
            }
        }

        private class NullableBclClass : BclClass
        {
            private readonly string _toString;
            private readonly string _parse;

            public NullableBclClass(
                string typeName,
                string typeAlias,
                Func<string, bool> isLegalValue,
                string toString,
                string parse)
                : base(typeName, typeAlias, isLegalValue)
            {
                _toString = toString;
                _parse = parse;
            }

            public override bool IsNullable { get { return true; } }

            public override string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes)
            {
                var sb = new StringBuilder();

                sb.AppendFormat(
@"[XmlIgnore]
public {0}? {1} {{ get; set; }}", _typeAlias, propertyName).AppendLine().AppendLine();

                foreach (var attribute in attributes)
                {
                    sb.AppendLine(string.Format("{0}", attribute.ToCode()));
                }

                sb.AppendFormat(
@"public string {0}String
{{
    get
    {{
        return {0} == null ? null : {0}{1};
    }}
    set
    {{
        {0} = value == null ? null : ({2}?){3};
    }}
}}", propertyName, _toString, _typeAlias, _parse);

                return sb.ToString();
            }
        }

        private class PascalCaseBooleanClass : BclClass
        {
            public PascalCaseBooleanClass()
                : base("PascalCaseBoolean", "bool", value => value == "True" || value == "False")
            {
            }

            public override string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes)
            {
                var sb = new StringBuilder();

                sb.AppendFormat(
                    @"[XmlIgnore]
public {0} {1} {{ get; set; }}", _typeAlias, propertyName).AppendLine().AppendLine();

                foreach (var attribute in attributes)
                {
                    sb.AppendLine(string.Format("{0}", attribute.ToCode()));
                }

                sb.AppendFormat(
 @"public string {0}String
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

        private class NullablePascalCaseBooleanClass : BclClass
        {
            public NullablePascalCaseBooleanClass()
                : base("PascalCaseBoolean", "bool", value => value == null || value == "True" || value == "False")
            {
            }

            public override bool IsNullable { get { return true; } }

            public override string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes)
            {
                var sb = new StringBuilder();

                sb.AppendFormat(
                    @"[XmlIgnore]
public {0} {1} {{ get; set; }}", _typeAlias, propertyName).AppendLine().AppendLine();

                foreach (var attribute in attributes)
                {
                    sb.AppendLine(string.Format("{0}", attribute.ToCode()));
                }

                sb.AppendFormat(
@"public string {0}String
{{
    get
    {{
        return {0} == null ? null : {0}.Value ? ""True"" : ""False"";
    }}
    set
    {{
        {0} = value == null ? null : (bool?)bool.Parse(value);
    }}
}}", propertyName);

                return sb.ToString();
            }
        }
    }
}

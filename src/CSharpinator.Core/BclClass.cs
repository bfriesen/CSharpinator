using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace CSharpinator
{
    [DebuggerDisplay("{TypeAlias}")]
    public class BclClass : BclClassBase
    {
        private static readonly ConcurrentDictionary<string, IBclClass> _classes = new ConcurrentDictionary<string, IBclClass>();

        protected BclClass(string typeName, string typeAlias, Func<string, bool> isLegalStringValue, Func<object, bool> isLegalObjectValue)
            : base(typeName, typeAlias, isLegalStringValue, isLegalObjectValue)
        {
        }

        public override bool IsNullable { get { return TypeAlias == String.TypeAlias; } }

        #region
        public static IBclClass String
        {
            get
            {
                return _classes.GetOrAdd(
                    typeof(string).FullName,
                    typeName => new BclClass(
                        typeName,
                        "string",
                        value => true,
                        value => true));
            }
        }

        public static IBclClass Boolean
        {
            get
            {
                return _classes.GetOrAdd(
                    typeof(bool).FullName,
                    typeName => new BclClass(
                        typeName,
                        "bool",
                        value => value == "true" || value == "false",
                        value => value is bool));
            }
        }

        public static IBclClass NullableBoolean
        {
            get
            {
                return _classes.GetOrAdd(
                    typeof(bool?).FullName,
                    typeName => new NullableBclClass(
                        typeName,
                        "bool?",
                        value => string.IsNullOrEmpty(value) || value == "true" || value == "false",
                        value => value == null || value is bool,
                        ".ToString().ToLower()",
                        "bool.Parse(value)"));
            }
        }

        public static IBclClass PascalCaseBoolean
        {
            get
            {
                return _classes.GetOrAdd(
                    "PascalCaseBoolean",
                    typeName => new PascalCaseBooleanClass());
            }
        }

        public static IBclClass NullablePascalCaseBoolean
        {
            get
            {
                return _classes.GetOrAdd(
                    "NullablePascalCaseBoolean",
                    typeName => new NullablePascalCaseBooleanClass());
            }
        }

        public static IBclClass Int32
        {
            get
            {
                return _classes.GetOrAdd(
                    typeof(int).FullName,
                    typeName => new BclClass(
                        typeName,
                        "int",
                        value => { int temp; return int.TryParse(value, out temp); },
                        value => IsIntegerType(value) && IsLegalInt32Value(value)));
            }
        }

        public static IBclClass NullableInt32
        {
            get
            {
                return _classes.GetOrAdd(
                    typeof(int?).FullName,
                    typeName => new NullableBclClass(
                        typeName,
                        "int?",
                        value => { int temp; return string.IsNullOrEmpty(value) || int.TryParse(value, out temp); },
                        value => value == null || (IsIntegerType(value) && IsLegalInt32Value(value)),
                        ".ToString()",
                        "int.Parse(value)"));
            }
        }

        public static IBclClass Int64
        {
            get
            {
                return _classes.GetOrAdd(
                    typeof(long).FullName,
                    typeName => new BclClass(
                        typeName,
                        "long", value => { long temp; return long.TryParse(value, out temp); },
                        value => IsIntegerType(value) && IsLegalInt64Value(value)));
            }
        }

        public static IBclClass NullableInt64
        {
            get
            {
                return _classes.GetOrAdd(
                    typeof(long?).FullName,
                    typeName => new NullableBclClass(
                        typeName,
                        "long?",
                        value => { long temp; return string.IsNullOrEmpty(value) || long.TryParse(value, out temp); },
                        value => value == null || (IsIntegerType(value) && IsLegalInt64Value(value)),
                        ".ToString()",
                        "long.Parse(value)"));
            }
        }

        public static IBclClass Decimal
        {
            get
            {
                return _classes.GetOrAdd(
                    typeof(decimal).FullName,
                    typeName => new BclClass(
                        typeName,
                        "decimal",
                        value => { decimal temp; return decimal.TryParse(value, out temp); },
                        value => value is decimal || value is double || value is float));
            }
        }

        public static IBclClass NullableDecimal
        {
            get
            {
                return _classes.GetOrAdd(
                    typeof(decimal?).FullName,
                    typeName => new NullableBclClass(
                        typeName,
                        "decimal?",
                        value => { decimal temp; return string.IsNullOrEmpty(value) || decimal.TryParse(value, out temp); },
                        value => value == null || value is decimal || value is double || value is float,
                        ".ToString()",
                        "decimal.Parse(value)"));
            }
        }

        public static IBclClass Guid
        {
            get
            {
                return _classes.GetOrAdd(
                    typeof(Guid).FullName,
                    typeName => new BclClass(
                        typeName,
                        "Guid",
                        value => { Guid temp; return System.Guid.TryParse(value, out temp); },
                        value => value is Guid));
            }
        }

        public static IBclClass NullableGuid
        {
            get
            {
                return _classes.GetOrAdd(
                    typeof(Guid?).FullName,
                    typeName => new NullableBclClass(
                        typeName,
                        "Guid?",
                        value => { Guid temp; return string.IsNullOrEmpty(value) || System.Guid.TryParse(value, out temp); },
                        value => value == null || value is Guid,
                        ".ToString()",
                        "Guid.Parse(value)"));
            }
        }

        private static bool IsIntegerType(object value)
        {
            return value is long || value is ulong || value is int || value is uint || value is short || value is ushort || value is byte || value is sbyte;
        }

        public static bool IsLegalInt32Value(object value)
        {
            return IsLegalInt32ValueImpl(value as dynamic);
        }

        private static bool IsLegalInt32ValueImpl(object value)
        {
            return false;
        }

        private static bool IsLegalInt32ValueImpl(long value)
        {
            return value >= int.MinValue && value <= int.MaxValue;
        }

        private static bool IsLegalInt32ValueImpl(ulong value)
        {
            return value <= int.MaxValue;
        }

        public static bool IsLegalInt64Value(object value)
        {
            return IsLegalInt64ValueImpl(value as dynamic);
        }

        private static bool IsLegalInt64ValueImpl(object value)
        {
            return false;
        }

        private static bool IsLegalInt64ValueImpl(long value)
        {
            return true;
        }

        private static bool IsLegalInt64ValueImpl(ulong value)
        {
            return value <= long.MaxValue;
        }
        #endregion
    }
}

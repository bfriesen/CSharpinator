using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CSharpifier
{
    [DebuggerDisplay("{TypeAlias}")]
    public class BclClass : BclClassBase
    {
        private static readonly ConcurrentDictionary<string, IBclClass> _classes = new ConcurrentDictionary<string, IBclClass>();

        protected BclClass(string typeName, string typeAlias, Func<string, bool> isLegalValue)
            : base(typeName, typeAlias, isLegalValue)
        {
        }
        
        public override bool IsNullable { get { return false; } }

        public override string GeneratePropertyCode(string propertyName, Case classCase, IEnumerable<AttributeProxy> attributes)
        {
            var sb = new StringBuilder();

            foreach (var attribute in attributes)
            {
                sb.AppendLine(string.Format("{0}", attribute.ToCode()));
            }

            sb.AppendFormat("public {0} {1} {{ get; set; }}", TypeAlias, propertyName);

            return sb.ToString();
        }

        #region
        public static IBclClass String
        {
            get { return _classes.GetOrAdd(typeof(string).FullName, typeName => new BclClass(typeName, "string", value => true)); }
        }

        public static IBclClass Boolean
        {
            get { return _classes.GetOrAdd(typeof(bool).FullName, typeName => new BclClass(typeName, "bool", value => value == "true" || value == "false")); }
        }

        public static IBclClass NullableBoolean
        {
            get { return _classes.GetOrAdd(typeof(bool?).FullName, typeName => new NullableBclClass(typeName, "bool?", value => string.IsNullOrEmpty(value) || value == "true" || value == "false", ".ToString().ToLower()", "bool.Parse(value)")); }
        }

        public static IBclClass PascalCaseBoolean
        {
            get { return _classes.GetOrAdd("PascalCaseBoolean", typeName => new PascalCaseBooleanClass()); }
        }

        public static IBclClass NullablePascalCaseBoolean
        {
            get { return _classes.GetOrAdd("NullablePascalCaseBoolean", typeName => new NullablePascalCaseBooleanClass()); }
        }

        public static IBclClass Int32
        {
            get { return _classes.GetOrAdd(typeof(int).FullName, typeName => new BclClass(typeName, "int", value => { int temp; return int.TryParse(value, out temp); })); }
        }

        public static IBclClass NullableInt32
        {
            get { return _classes.GetOrAdd(typeof(int?).FullName, typeName => new NullableBclClass(typeName, "int?", value => { int temp; return string.IsNullOrEmpty(value) || int.TryParse(value, out temp); }, ".ToString()", "int.Parse(value)")); }
        }

        public static IBclClass Int64
        {
            get { return _classes.GetOrAdd(typeof(long).FullName, typeName => new BclClass(typeName, "long", value => { long temp; return long.TryParse(value, out temp); })); }
        }

        public static IBclClass NullableInt64
        {
            get { return _classes.GetOrAdd(typeof(long?).FullName, typeName => new NullableBclClass(typeName, "long?", value => { long temp; return string.IsNullOrEmpty(value) || long.TryParse(value, out temp); }, ".ToString()", "long.Parse(value)")); }
        }

        public static IBclClass Decimal
        {
            get { return _classes.GetOrAdd(typeof(decimal).FullName, typeName => new BclClass(typeName, "decimal", value => { decimal temp; return decimal.TryParse(value, out temp); })); }
        }

        public static IBclClass NullableDecimal
        {
            get { return _classes.GetOrAdd(typeof(decimal?).FullName, typeName => new NullableBclClass(typeName, "decimal?", value => { decimal temp; return string.IsNullOrEmpty(value) || decimal.TryParse(value, out temp); }, ".ToString()", "decimal.Parse(value)")); }
        }

        public static IBclClass Guid
        {
            get { return _classes.GetOrAdd(typeof(Guid).FullName, typeName => new BclClass(typeName, "Guid", value => { Guid temp; return System.Guid.TryParse(value, out temp); })); }
        }

        public static IBclClass NullableGuid
        {
            get { return _classes.GetOrAdd(typeof(Guid?).FullName, typeName => new NullableBclClass(typeName, "Guid?", value => { Guid temp; return string.IsNullOrEmpty(value) || System.Guid.TryParse(value, out temp); }, ".ToString()", "Guid.Parse(value)")); }
        }
        #endregion
    }
}

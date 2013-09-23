using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CSharpinator
{
    public abstract class FormattedBooleanBase : BclClassBase
    {
        private static readonly ConcurrentDictionary<string, ICollection<BooleanFormat>> _formats = new ConcurrentDictionary<string, ICollection<BooleanFormat>>();

        private readonly string _formatString;

        protected FormattedBooleanBase(string formatString, Func<string, bool> isLegalValue)
            : base("FormattedBoolean", "bool", isLegalValue)
        {
            _formatString = formatString;
        }

        public string FormatString
        {
            get { return _formatString; }
        }

        public override bool Equals(object other)
        {
            var otherFormattedBooleanBase = other as FormattedBooleanBase;
            if (otherFormattedBooleanBase == null)
            {
                return false;
            }

            return Equals(otherFormattedBooleanBase);
        }

        protected abstract bool Equals(FormattedBooleanBase other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (TypeName.GetHashCode() * 397) ^ FormatString.GetHashCode();
            }
        }

        protected static ICollection<BooleanFormat> GetFormats(string formatString)
        {
            return _formats.GetOrAdd(
                formatString,
                f =>
                f.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                 .Select(
                     x =>
                     {
                         var split = x.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                         return new BooleanFormat { True = split[0], False = split[1] };
                     }).ToList());
        }

        protected static string GetNonNullableSetter(ICollection<BooleanFormat> formats, string valueVariable)
        {
            var aggregatedFormats = formats.Aggregate(
                Tuple.Create(new List<string>(), new List<string>()),
                (t, f) =>
                {
                    t.Item1.Add(f.True);
                    t.Item2.Add(f.False);
                    return t;
                });

            var validValues = formats.SelectMany(format => new[] { "'" + format.True + "'", "'" + format.False + "'" }).ToList();

            return string.Format(
@"        if ({0})
        {{
            {2} = true;
        }}
        else if ({1})
        {{
            {2} = false;
        }}
        else
        {{
            throw new InvalidOperationException(string.Format(""Unable to serialize value for {2}: '{{0}}'. Valid values are: {3}."", {2}));
        }}",
                GetComparisons("value", aggregatedFormats.Item1),
                GetComparisons("value", aggregatedFormats.Item2),
                valueVariable,
                string.Join(", ", validValues.Take(validValues.Count - 1)) + ", and " + validValues.Last());
        }

        private static string GetComparisons(string variableName, IEnumerable<string> comparisonValues)
        {
            return string.Join(
                " || ",
                comparisonValues.Select(
                    comparisonValue => string.Format("{0} == \"{1}\"", variableName, comparisonValue)));
        }

        protected class BooleanFormat
        {
            public string True { get; set; }
            public string False { get; set; }
        }
    }
}
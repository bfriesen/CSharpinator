using System;

namespace CSharpinator
{
    public class BooleanFormat
    {
        public string True { get; set; }
        public string False { get; set; }

        public static BooleanFormat FromFormatString(string formatString)
        {
            if (formatString == null)
            {
                throw new ArgumentNullException("formatString");
            }

            var split = formatString.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length != 2)
            {
                throw new ArgumentException("Invalid format string for construction of a BooleanFormat: " + formatString);
            }

            return new BooleanFormat
            {
                True = split[0],
                False = split[1]
            };
        }

        protected bool Equals(BooleanFormat other)
        {
            return string.Equals(True, other.True) && string.Equals(False, other.False);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((BooleanFormat)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (True.GetHashCode() * 397) ^ False.GetHashCode();
            }
        }
    }
}
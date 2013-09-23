using System.Collections.Generic;

namespace CSharpinator
{
    public class Configuration : IConfiguration
    {
        private readonly HashSet<string> _dateTimeFormats = new HashSet<string>();
        private readonly HashSet<BooleanFormat> _booleanFormats = new HashSet<BooleanFormat>();

        public ICollection<string> DateTimeFormats
        {
            get { return _dateTimeFormats; }
        }

        public ICollection<BooleanFormat> BooleanFormats
        {
            get { return _booleanFormats; }
        }
    }
}
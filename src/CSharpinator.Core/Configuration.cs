using System.Collections.Generic;

namespace CSharpinator
{
    public class Configuration : IConfiguration
    {
        private readonly HashSet<string> _dateTimeFormats = new HashSet<string>();

        public ICollection<string> DateTimeFormats
        {
            get { return _dateTimeFormats; }
        }
    }
}
using System.Collections.Generic;

namespace CSharpifier
{
    public interface IConfiguration
    {
        ICollection<string> DateTimeFormats { get; }
    }
}
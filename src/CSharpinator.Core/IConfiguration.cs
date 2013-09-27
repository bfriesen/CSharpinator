using System.Collections.Generic;

namespace CSharpinator
{
    public interface IConfiguration
    {
        ICollection<string> DateTimeFormats { get; }
        string JsonRootElementName { get; }
    }
}
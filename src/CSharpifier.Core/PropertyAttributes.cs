using System;

namespace CSharpifier
{
    [Flags]
    public enum PropertyAttributes
    {
        None            = 0x00,
        XmlSerializion  = 0x01,
        DataContract    = 0x02
    }
}

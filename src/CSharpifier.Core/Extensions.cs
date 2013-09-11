using System;
using System.Linq;

namespace CSharpifier
{
    public static class Extensions
    {
         public static string Indent(this string value)
         {
             return string.Join("\r\n", value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Select(x => "    " + x));
         }
    }
}
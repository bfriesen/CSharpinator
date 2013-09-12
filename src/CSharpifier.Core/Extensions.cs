using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpifier
{
    public static class Extensions
    {
        public static string Indent(this string value)
        {
            return string.Join("\r\n", value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Select(x => "    " + x));
        }

        public static IEnumerable<UserDefinedClass> GetUsedClasses(this IClassRepository repository)
        {
            return repository.GetAll().First().GetUsedClasses();
        }

        public static IEnumerable<UserDefinedClass> GetUsedClasses(this UserDefinedClass rootClass)
        {
            return GetUserDefinedClassesImpl(rootClass).Distinct();
        }

        private static IEnumerable<UserDefinedClass> GetUserDefinedClassesImpl(UserDefinedClass @class)
        {
            yield return @class;

            foreach (var childClass in @class.Properties
                .Select(x => x.SelectedPropertyDefinition)
                .Where(x => IsUserDefinedClass(x.Class))
                .Select(x => GetUserDefinedClass(x.Class)))
            {
                foreach (var grandchildClass in GetUsedClasses(childClass))
                {
                    yield return grandchildClass;
                }
            }
        }

        private static bool IsUserDefinedClass(Class @class)
        {
            if (@class is UserDefinedClass)
            {
                return true;
            }

            if (@class is BclClass)
            {
                return false;
            }

            return IsUserDefinedClass(((ListClass)@class).Class);
        }

        private static UserDefinedClass GetUserDefinedClass(Class @class)
        {
            if (@class is UserDefinedClass)
            {
                return (UserDefinedClass)@class;
            }

            return GetUserDefinedClass(((ListClass)@class).Class);
        }
    }
}
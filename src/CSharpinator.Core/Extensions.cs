using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace CSharpinator
{
    public static class Extensions
    {
        private static string GetXPath(this XText text)
        {
            return text.Parent.GetXPath();
        }

        private static string GetXPath(this XAttribute attribute)
        {
            return attribute.Parent.GetXPath() + "/" + attribute.Name.LocalName;
        }

        private static string GetXPath(this XElement element)
        {
            return string.Join("/", element.AncestorsAndSelf().Select(x => x.Name.LocalName).Reverse());
        }

        private static string GetPath(this JToken jToken, string rootElementName)
        {
            var path = Regex.Replace(jToken.Path, @"\[\d+\]", "").Replace(".", "/");

            if (string.IsNullOrEmpty(path))
            {
                return rootElementName;
            }

            return rootElementName + "/" + path;
        }

        public static DomPath GetDomPath(this XText text, IFactory factory)
        {
            return factory.GetOrCreateDomPath(text.GetXPath());
        }

        public static DomPath GetDomPath(this XAttribute attribute, IFactory factory)
        {
            return factory.GetOrCreateDomPath(attribute.GetXPath());
        }

        public static DomPath GetDomPath(this XElement element, IFactory factory)
        {
            return factory.GetOrCreateDomPath(element.GetXPath());
        }

        public static DomPath GetDomPath(this JToken jToken, IFactory factory)
        {
            return factory.GetOrCreateDomPath(jToken.GetPath(factory.JsonRootElementName));
        }

        public static string Indent(this string value)
        {
            return string.Join("\r\n", value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Select(x => "    " + x));
        }

        public static IBclClass AsBclClass(this IClass @class)
        {
            return @class.AsClass<IBclClass>();
        }

        public static UserDefinedClass AsUserDefinedClass(this IClass @class)
        {
            return @class.AsClass<UserDefinedClass>();
        }

        private static TClass AsClass<TClass>(this IClass @class)
            where TClass : class, IClass
        {
            var tClass = @class as TClass;
            if (tClass != null)
            {
                return tClass;
            }

            var listClass = @class as ListClass;
            if (listClass != null)
            {
                return listClass.Class.AsClass<TClass>();
            }

            return null;
        }

        public static bool IsBclClass(this IClass @class)
        {
            return @class.IsClass<IBclClass>();
        }

        public static bool IsUserDefinedClass(this IClass @class)
        {
            return @class.IsClass<UserDefinedClass>();
        }

        private static bool IsClass<TClass>(this IClass @class)
            where TClass : IClass
        {
            if (@class is TClass)
            {
                return true;
            }

            var listClass = @class as ListClass;
            if (listClass != null)
            {
                return listClass.Class.IsClass<TClass>();
            }

            return false;
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
                .Where(x => x != null && x.Class.IsUserDefinedClass())
                .Select(x => x.Class.AsUserDefinedClass()))
            {
                foreach (var grandchildClass in GetUserDefinedClassesImpl(childClass))
                {
                    yield return grandchildClass;
                }
            }
        }
    }
}
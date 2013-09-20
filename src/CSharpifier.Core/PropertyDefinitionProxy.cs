using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace CSharpifier
{
    [XmlRoot("PropertyDefinition")]
    public class PropertyDefinitionProxy
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlIgnore]
        public ClassProxy Class { get; set; }
        [XmlIgnore]
        public List<AttributeProxy> Attributes { get; set; }
        [XmlAttribute]
        public bool IsLegal { get; set; }
        [XmlAttribute]
        public bool IsEnabled { get; set; }

        [XmlAttribute("Class")]
        public string ClassString
        {
            get { return GetClassString(Class); }
            set { Class = GetClassFromString(value); }
        }

        [XmlAttribute("Attributes")]
        public string AttributesString
        {
            get { return GetAttributesString(Attributes); }
            set { Attributes = GetAttributesFromString(value); }
        }

        private static string GetClassString(ClassProxy @class)
        {
            var userDefined = @class as UserDefinedClassProxy;
            if (userDefined != null)
            {
                return "UserDefined:" + userDefined.DomPath;
            }

            var formattedDateTime = @class as FormattedDateTimeProxy;
            if (formattedDateTime != null)
            {
                return "FormattedDateTime:" + formattedDateTime.Format;
            }

            var nullableFormattedDateTime = @class as NullableFormattedDateTimeProxy;
            if (nullableFormattedDateTime != null)
            {
                return "NullableFormattedDateTime:" + nullableFormattedDateTime.Format;
            }

            var bcl = @class as BclClassProxy;
            if (bcl != null)
            {
                var typeName = bcl.TypeName;

                var type = Type.GetType(typeName);
                if (type != null && type.IsGenericType)
                {
                    var genericArgs = type.GetGenericArguments();
                    if (genericArgs.Length == 1 && type == typeof(Nullable<>).MakeGenericType(genericArgs[0]))
                    {
                        typeName = string.Format("Nullable({0})", genericArgs[0].FullName);
                    }
                }

                return "Bcl:" + bcl.TypeAlias + ":" + typeName;
            }

            var list = @class as ListClassProxy;
            if (list != null)
            {
                return "List:" + GetClassString(list.Class);
            }

            throw new InvalidOperationException("Invalid class type: " + @class.GetType());
        }

        private static ClassProxy GetClassFromString(string classString)
        {
            var firstColonIndex = classString.IndexOf(':');
            var classType = classString.Substring(0, firstColonIndex);
            var remainder = classString.Substring(firstColonIndex + 1);

            switch (classType)
            {
                case "UserDefined":
                {
                    return new UserDefinedClassProxy
                    {
                        Properties = new List<PropertyProxy>(),
                        DomPath = remainder
                    };
                }
                case "FormattedDateTime":
                {
                    return new FormattedDateTimeProxy { Format = remainder };
                }
                case "NullableFormattedDateTime":
                {
                    return new NullableFormattedDateTimeProxy { Format = remainder };
                }
                case "Bcl":
                {
                    var split = remainder.Split(':');

                    var typeName = split[1];
                    var nullableMatch = Regex.Match(typeName, @"Nullable\(([^)]+)\)");
                    if (nullableMatch.Success)
                    {
                        var genericArg = Type.GetType(nullableMatch.Groups[1].Value);
                        if (genericArg != null)
                        {
                            typeName = typeof(Nullable<>).MakeGenericType(genericArg).FullName;
                        }
                    }

                    return new BclClassProxy
                    {
                        TypeAlias = split[0],
                        TypeName = typeName
                    };
                }
                case "List":
                {
                    return new ListClassProxy
                    {
                        Class = GetClassFromString(remainder)
                    };
                }
                default:
                {
                    throw new InvalidOperationException("Invalid class type: " + classType);
                }
            }
        }

        private string GetAttributesString(List<AttributeProxy> attributes)
        {
            string attributesString = string.Join("|", attributes.Select(x => x.AttributeTypeName + (string.IsNullOrEmpty(x.ElementNameSetter) ? "" : ":" + x.ElementNameSetter)));
            return attributesString;
        }

        private static List<AttributeProxy> GetAttributesFromString(string value)
        {
            var attributes = new List<AttributeProxy>(value.Split('|').Select(x =>
            {
                var split = x.Split(':');
                var elementNameSetter = split.Length == 2 ? split[1] : null;

                return new AttributeProxy
                {
                    AttributeTypeName = split[0],
                    ElementNameSetter = elementNameSetter
                };
            }));
            return attributes;
        }

        public static PropertyDefinitionProxy FromPropertyDefinition(PropertyDefinition propertyDefinition)
        {
            return new PropertyDefinitionProxy
            {
                Name = propertyDefinition.Name.Raw,
                Class = ClassProxy.FromClass(propertyDefinition.Class),
                Attributes = new List<AttributeProxy>(propertyDefinition.Attributes),
                IsLegal = propertyDefinition.IsLegal,
                IsEnabled = propertyDefinition.IsEnabled
            };
        }

        public PropertyDefinition ToPropertyDefinition(IClassRepository classRepository, IFactory factory)
        {
            var @class = Class.ToClass(classRepository, factory);
            var propertyDefinition = factory.CreatePropertyDefinition(@class, Name, IsLegal, IsEnabled, Attributes.ToArray());
            return propertyDefinition;
        }
    }
}

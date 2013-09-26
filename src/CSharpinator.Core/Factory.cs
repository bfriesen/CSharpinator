using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace CSharpinator
{
    public class Factory : IFactory
    {
        private readonly ConcurrentDictionary<string, FormattedDateTime> _formattedDateTimes = new ConcurrentDictionary<string, FormattedDateTime>();
        private readonly ConcurrentDictionary<string, NullableFormattedDateTime> _nullableFormattedDateTimes = new ConcurrentDictionary<string, NullableFormattedDateTime>();

        private readonly IConfiguration _configuration;

        public Factory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public XmlDomElement CreateXmlDomElement(XElement element)
        {
            return new XmlDomElement(element, this);
        }

        public XmlDomAttribute CreateXmlDomAttribute(XAttribute attribute)
        {
            return new XmlDomAttribute(attribute, this);
        }

        public XmlDomText CreateXmlDomText(XText text)
        {
            return new XmlDomText(text, this);
        }

        public Property CreateProperty(DomPath domPath, bool isNonEmpty)
        {
            return new Property(domPath, isNonEmpty, this);
        }

        public PropertyDefinition CreatePropertyDefinition(IClass @class, string propertyName, bool isLegal, bool isEnabled, params AttributeProxy[] attributes)
        {
            return new PropertyDefinition(@class, propertyName, isLegal, isEnabled, this)
            {
                Attributes = attributes.ToList()
            };
        }

        public IEnumerable<IBclClass> GetAllBclClasses()
        {
            yield return GetOrCreateFormattedDateTime("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK");
            yield return GetOrCreateNullableFormattedDateTime("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK");

            foreach (var dateTimeFormat in _configuration.DateTimeFormats)
            {
                yield return GetOrCreateFormattedDateTime(dateTimeFormat);
                yield return GetOrCreateNullableFormattedDateTime(dateTimeFormat);
            }

            yield return BclClass.Int32;
            yield return BclClass.NullableInt32;
            yield return BclClass.Int64;
            yield return BclClass.NullableInt64;
            yield return BclClass.Decimal;
            yield return BclClass.NullableDecimal;
            yield return BclClass.Boolean;
            yield return BclClass.NullableBoolean;
            yield return BclClass.PascalCaseBoolean;
            yield return BclClass.NullablePascalCaseBoolean;
            yield return BclClass.Guid;
            yield return BclClass.NullableGuid;
            yield return BclClass.String;
        }

        public FormattedDateTime GetOrCreateFormattedDateTime(string format)
        {
            return _formattedDateTimes.GetOrAdd(format, f => new FormattedDateTime(format, this));
        }

        public NullableFormattedDateTime GetOrCreateNullableFormattedDateTime(string format)
        {
            return _nullableFormattedDateTimes.GetOrAdd(format, f => new NullableFormattedDateTime(format, this));
        }

        public IBclClass GetBclClassFromTypeName(string typeName)
        {
            if (typeName == "PascalCaseBoolean")
            {
                return BclClass.PascalCaseBoolean;
            }

            if (typeName == "NullablePascalCaseBoolean")
            {
                return BclClass.NullablePascalCaseBoolean;
            }

            if (typeName.StartsWith("FormattedDateTime"))
            {
                return GetOrCreateFormattedDateTime(typeName.Substring(typeName.IndexOf(":")));
            }

            if (typeName.StartsWith("NullableFormattedDateTime"))
            {
                return GetOrCreateNullableFormattedDateTime(typeName.Substring(typeName.IndexOf(":")));
            }

            var type = Type.GetType(typeName);

            if (type == typeof(string))
            {
                return BclClass.String;
            }

            if (type == typeof(bool))
            {
                return BclClass.Boolean;
            }

            if (type == typeof(bool?))
            {
                return BclClass.NullableBoolean;
            }

            if (type == typeof(int))
            {
                return BclClass.Int32;
            }

            if (type == typeof(int?))
            {
                return BclClass.NullableInt32;
            }

            if (type == typeof(long))
            {
                return BclClass.Int64;
            }

            if (type == typeof(long?))
            {
                return BclClass.NullableInt64;
            }

            if (type == typeof(decimal))
            {
                return BclClass.Decimal;
            }

            if (type == typeof(decimal?))
            {
                return BclClass.NullableDecimal;
            }

            if (type == typeof(Guid))
            {
                return BclClass.Guid;
            }

            if (type == typeof(Guid?))
            {
                return BclClass.NullableGuid;
            }

            throw new InvalidOperationException("Invalid type for BclClass: " + type);
        }

        private readonly Dictionary<string, DomPath> domPaths = new Dictionary<string, DomPath>();
        private readonly HashSet<string> stuff = new HashSet<string>();

        public DomPath GetOrCreateDomPath(string fullPath)
        {
            if (domPaths.ContainsKey(fullPath))
            {
                return domPaths[fullPath];
            }

            for (int i = 0;; i++)
            {
                var domPath = new DomPath(fullPath, i);
                if (!stuff.Contains(domPath.TypeName.Raw))
                {
                    stuff.Add(domPath.TypeName.Raw);
                    domPaths.Add(fullPath, domPath);
                    return domPath;
                }
            }

            // If there is one that exists with the given fullPath, return it.
            // Start with a depth of zero.
            //     If there is not one that exists at the current depth, create and return one at that depth
            //     Increment depth
        }

        public DomPath GetOrCreateDomPath(string fullPath, int typeNameDepth)
        {
            if (domPaths.ContainsKey(fullPath))
            {
                return domPaths[fullPath];
            }

            var domPath = new DomPath(fullPath, typeNameDepth);
            if (stuff.Contains(domPath.TypeName.Raw))
            {
                throw new InvalidOperationException("Invalid metadata: more than one user defined class with identical type names.");
            }

            stuff.Add(domPath.TypeName.Raw);
            domPaths.Add(fullPath, domPath);
            return domPath;
        }

        public JObjectDomElement CreateJObjectDomElement(JObject jObject)
        {
            return new JObjectDomElement(jObject, this);
        }

        public JObjectDomElement CreateJObjectDomElement(JProperty jProperty)
        {
            if (jProperty.Value.Type != JTokenType.Object)
            {
                throw new ArgumentException("A JProperty passed in to CreateJObjectDomElement must have a value of Type JTokenType.Object.", "jProperty");
            }

            return new JObjectDomElement((JObject)jProperty.Value, jProperty.Name, this);
        }

        public IDomElement CreateJsonDomElement(JToken jToken, string name)
        {
            switch (jToken.Type)
            {
                case JTokenType.Object:
                    return new JObjectDomElement((JObject)jToken, name, this);
                case JTokenType.Array:
                    return new JArrayDomElement((JArray)jToken, name, this);
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.Date:
                case JTokenType.Boolean:
                case JTokenType.String:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                    return new JValueDomElement((JValue)jToken, name, this);
                case JTokenType.Constructor:
                    break;
                case JTokenType.Property:
                    break;
                case JTokenType.Comment:
                    break;
                case JTokenType.Null:
                    break;
                case JTokenType.Undefined:
                    break;
                case JTokenType.Raw:
                    break;
                case JTokenType.Bytes:
                    break;
                case JTokenType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotImplementedException("Factory.CreateJsonDomElement has not been implemented for JTokenType." + jToken.Type);
        }

        public IDomElement CreateJsonDomElement(JProperty jProperty)
        {
            return CreateJsonDomElement(jProperty.Value, jProperty.Name);
        }
    }
}
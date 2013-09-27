using Newtonsoft.Json.Linq;

namespace CSharpinator
{
    public static class FactoryExtensions
    {
        public static IDomElement CreateJsonDomElement(this IFactory factory, JToken jToken)
        {
            return factory.CreateJsonDomElement(jToken, factory.JsonRootElementName);
        }

        public static IDomElement CreateJsonDomElement(this IFactory factory, JProperty jProperty)
        {
            return factory.CreateJsonDomElement(jProperty.Value, jProperty.Name);
        }
    }
}
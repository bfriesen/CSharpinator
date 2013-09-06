<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Xml.Serialization</Namespace>
</Query>

#define NONEST
void Main()
{
    var xmlDocument = XDocument.Load(@"C:\Temp\xml_to_classes\test.xml");
    
    Traverse(xmlDocument.Root, null);
    string.Join(
        "\r\n\r\n/**********************************************************/\r\n\r\n",
        UserDefinedClass.GetAllUsedClasses().Select(x => x.GenerateCSharpCode("Test"))).Dump();
}

public void Traverse(XElement element, UserDefinedClass currentClass)
{
    if (!element.HasElements && !element.HasAttributes)
    {
        if (string.IsNullOrEmpty(element.Value))
        {
            if (currentClass == null)
            {
                currentClass = UserDefinedClass.Create(element.Name);
            }
            else
            {
                var property = new Property(element.Name);
                property.AddPotentialPropertyDefinitions(BclClass.GetAll().Select(x =>
                    new PropertyDefinition(x, element.Name)
                    {
                        XmlElement = new XmlElementAttribute(element.Name.ToString())
                    }));
                property.AddPotentialPropertyDefinition(
                    new PropertyDefinition(UserDefinedClass.Create(element.Name), element.Name)
                    {
                        XmlElement = new XmlElementAttribute(element.Name.ToString())
                    });
                currentClass.AddProperty(property);
            }
        }
        else
        {
            if (currentClass == null)
            {
                currentClass = UserDefinedClass.Create(element.Name);
            }
            
            var property = new Property(element.Name);
            property.AddPotentialPropertyDefinition(
                new PropertyDefinition(BclClass.FromValue(element.Value), element.Name)
                {
                    XmlElement = new XmlElementAttribute(element.Name.ToString())
                });
            currentClass.AddProperty(property);
        }
    }
    else
    {
        if (currentClass != null)
        {
            var property = new Property(element.Name);
            property.AddPotentialPropertyDefinition(
                new PropertyDefinition(UserDefinedClass.Create(element.Name), element.Name)
                {
                    XmlElement = new XmlElementAttribute(element.Name.ToString())
                });
            currentClass.AddProperty(property);
        }
    
        currentClass = UserDefinedClass.Create(element.Name);
                
        if (element.HasAttributes)
        {
            foreach (var attribute in element.Attributes())
            {
                var property = new Property(attribute.Name);
                property.AddPotentialPropertyDefinition(
                    new PropertyDefinition(BclClass.FromValue(attribute.Value), attribute.Name)
                    {
                        XmlAttribute = new XmlAttributeAttribute(attribute.Name.ToString())
                    });
                currentClass.AddProperty(property);
            }
            
            if (!element.HasElements && !string.IsNullOrEmpty(element.Value))
            {
                var property = new Property("Value");
                property.AddPotentialPropertyDefinition(
                    new PropertyDefinition(BclClass.FromValue(element.Value), "Value")
                    {
                        XmlText = new XmlTextAttribute()
                    });
                currentClass.AddProperty(property);
            }
        }
        
        if (element.HasElements)
        {
            foreach (var childElement in element.Elements())
            {
                Traverse(childElement, currentClass);
            }
        }
    }
}

public abstract class Class
{
    public abstract string GeneratePropertyCode(string propertyName);
}

public class UserDefinedClass : Class
{
    private static readonly ConcurrentDictionary<string, UserDefinedClass> _classes = new ConcurrentDictionary<string, UserDefinedClass>();
    
    private readonly Dictionary<string, Property> _properties = new Dictionary<string, Property>();
    private readonly string _typeName;

    private UserDefinedClass(string typeName)
    {
        _typeName = typeName;
    }

    public static UserDefinedClass Create(XName typeName)
    {
        return _classes.GetOrAdd(typeName.ToString(), x => new UserDefinedClass(x));
    }
    
    public static IEnumerable<UserDefinedClass> GetAllUsedClasses()
    {
        return _classes.Values.Where(x =>
        {
            return true;
        });
    }
    
    public string TypeName
    {
        get { return _typeName; }
    }
    
    public IEnumerable<Property> Properties
    {
        get { return _properties.Values; }
    }
    
    public void AddProperty(Property property)
    {
        Property foundProperty;
        if (!_properties.TryGetValue(property.Name, out foundProperty))
        {
            _properties.Add(property.Name, property);
            return;
        }
        
        foundProperty.AddPotentialPropertyDefinitions(property.PotentialPropertyDefinitions);
    }
    
    public string GenerateCSharpCode(string @namespace)
    {
        return string.Format(
@"using System;
using System.Xml.Serialization;

namespace {0}
{{
    public class {1}
    {{
{2}
    }}
}}",
            @namespace,
            _typeName,
            string.Join("\r\n\r\n", Properties.Select(x => x.GeneratePropertyCode())));
    }
    
    public override string GeneratePropertyCode(string propertyName)
    {
        return string.Format("public {0} {1} {{ get; set; }}", _typeName, propertyName);
    }
}

public class BclClass : Class
{
    private static readonly ConcurrentDictionary<Type, BclClass> _classes = new ConcurrentDictionary<Type, BclClass>();
    private readonly Type _type;

    private BclClass(Type type)
    {
        _type = type;
    }

    public override string GeneratePropertyCode(string propertyName)
    {
        return string.Format("public {0} {1} {{ get; set; }}", _type.Name, propertyName);
    }
    
    public string TypeName { get { return _type.Name; } }
    
    public static BclClass String
    {
        get { return _classes.GetOrAdd(typeof(string), type => new BclClass(type)); }
    }
    
    public static BclClass Boolean
    {
        get { return new BclClass(typeof(bool)); }
    }
    
    public static BclClass Int32
    {
        get { return new BclClass(typeof(int)); }
    }
    
    public static BclClass Decimal
    {
        get { return _classes.GetOrAdd(typeof(decimal), type => new BclClass(type)); }
    }
    
    public static BclClass DateTime
    {
        get { return _classes.GetOrAdd(typeof(DateTime), type => new BclClass(type)); }
    }
    
    public static BclClass Guid
    {
        get { return _classes.GetOrAdd(typeof(Guid), type => new BclClass(type)); }
    }
    
    public static IEnumerable<BclClass> GetAll()
    {
        yield return String;
        yield return Boolean;
        yield return Int32;
        yield return Decimal;
        yield return DateTime;
        yield return Guid;
    }
    
    public static BclClass FromValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return String;
        }
        
        int tempInt;
        if (int.TryParse(value, out tempInt))
        {
            return Int32;
        }
        
        bool tempBool;
        if (bool.TryParse(value, out tempBool))
        {
            return Boolean;
        }
        
        decimal tempDecimal;
        if (decimal.TryParse(value, out tempDecimal))
        {
            return Decimal;
        }
        
        System.Guid tempGuid;
        if (System.Guid.TryParse(value, out tempGuid))
        {
            return Guid;
        }
        
        System.DateTime tempDateTime;
        if (System.DateTime.TryParse(value, out tempDateTime))
        {
            return DateTime;
        }
        
        return String;
    }
}

public class Property
{
    private readonly List<PropertyDefinition> _potentialPropertyDefinitions = new List<PropertyDefinition>();

    public Property(XName propertyName)
    {
        Name = propertyName.ToString();
    }

    public string Name { get; set; }
    
    public IEnumerable<PropertyDefinition> PotentialPropertyDefinitions
    {
        get { return _potentialPropertyDefinitions; }
    }
    
    public void AddPotentialPropertyDefinition(PropertyDefinition potentialPropertyDefinition)
    {
        if (_potentialPropertyDefinitions.Any(x => x.Class == potentialPropertyDefinition.Class))
        {
            return;
        }
        
        _potentialPropertyDefinitions.Add(potentialPropertyDefinition);
    }
    
    public void AddPotentialPropertyDefinitions(IEnumerable<PropertyDefinition> otherPotentialPropertyDefinitions)
    {
        foreach (var otherPotentialPropertyDefinition in otherPotentialPropertyDefinitions)
        {
            AddPotentialPropertyDefinition(otherPotentialPropertyDefinition);
        }
    }
    
    public string GeneratePropertyCode()
    {
        return PotentialPropertyDefinitions.First().GeneratePropertyCode();
    }
}

public class PropertyDefinition
{
    public PropertyDefinition(Class @class, XName propertyName)
    {
        Class = @class;
        Name = propertyName.ToString();
    }

    public Class Class { get; set; }
    public string Name { get; set; }
    public XmlElementAttribute XmlElement { get; set; }
    public XmlAttributeAttribute XmlAttribute { get; set; }
    public XmlTextAttribute XmlText { get; set; }
    
    public string GeneratePropertyCode()
    {
        var sb = new StringBuilder();
        
        if (XmlElement != null)
        {
            sb.AppendLine(string.Format("        [XmlElement(\"{0}\")]", Name));
        }
        
        if (XmlAttribute != null)
        {
            sb.AppendLine(string.Format("        [XmlAttribute(\"{0}\")]", Name));
        }
        
        if (XmlText != null)
        {
            sb.AppendLine("        [XmlText]");
        }
        
        sb.AppendFormat("        {0}", Class.GeneratePropertyCode(Name));
        
        return sb.ToString();
    }
}
<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Xml.Serialization</Namespace>
</Query>

#define NONEST
void Main(string[] args)
{
    XDocument xDocument;

    if (args == null)
    {
        xDocument = XDocument.Parse(
@"<Foo>
  <Bar>
    <Baz baz_value=""123"">1.23</Baz>
    <Boom>abc</Boom>
    <Bang something=""1978-01-29"">94e0131b-10ef-461d-b52c-045552dcc78f</Bang>
    <Fail />
  </Bar>
</Foo>");
    }
    else
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Must include path to document as first argument.");
            return;
        }
        
        if (!File.Exists(args[0]))
        {
            Console.WriteLine("No file exists at: " + args[0]);
            return;
        }
        
        try
        {            
            xDocument = XDocument.Load(args[0]);
        }
        catch
        {
            Console.WriteLine("Error reading into XDocument for file: " + args[0]);
            return;
        }
    }

    var domElement = new XmlDomElement(xDocument.Root);
    
    var classRepository = new ClassRepository();
    
    var domVisitor = new DomVisitor(classRepository);
    domVisitor.Visit(domElement);
    
//    var classes = classRepository.GetAll();
//    classes.Dump();
//    
//    var classDefinitions = ClassDefinitions.FromClasses(classes);
//    classDefinitions.Dump();
//    
//    var loadedClasses = classDefinitions.ToClasses(classRepository);
//    loadedClasses.Dump();
    
    var classGenerator = new ClassGenerator(classRepository);
    classGenerator.Write(
        Case.PascalCase,
        Case.PascalCase,
        PropertyAttributes.XmlSerializion | PropertyAttributes.DataContract,
        Console.Out);
}

public class DomVisitor
{
    private IClassRepository _classRepository;
    
    public DomVisitor(IClassRepository classRepository)
    {
        _classRepository = classRepository;
    }

    public bool ExcludeNamespace { get; set; }

    public void Visit(IDomElement element)
    {
        Visit(element, null);
    }

    private void Visit(IDomElement element, UserDefinedClass currentClass)
    {
        if (element.HasElements) // if element has child elements
        {
            if (currentClass != null) // if this is the root element
            {
                var property = element.CreateProperty();
                currentClass.AddProperty(property);
            }
        
            currentClass = new UserDefinedClass(element.Name);
            _classRepository.AddOrUpdate(currentClass);
            
            foreach (var childElement in element.Elements)
            {
                Visit(childElement, currentClass);
            }
        }
        else // if element has no child elements
        {
            if (currentClass == null) // if this is the root element
            {
                currentClass = new UserDefinedClass(element.Name);
                _classRepository.AddOrUpdate(currentClass);
            }
            else // if this is not the root element
            {
                var property = element.CreateProperty();
                currentClass.AddProperty(property);
            }
        }
    }
}

public interface IDomElement
{
    bool HasElements { get; }
    string Value { get; }
    string Name { get; }
    IEnumerable<IDomElement> Elements { get; }
    Property CreateProperty();
}

public class XmlDomElement : IDomElement
{
    private readonly XElement _element;

    public XmlDomElement(XElement element)
    {
        _element = element;
    }
    
    public bool HasElements
    {
        get { return _element.HasElements || _element.HasAttributes; }
    }
    
    public string Value
    {
        get { return _element.Value; }
    }
    
    public string Name
    {
        get { return _element.Name.ToString(); }
    }
    
    public IEnumerable<IDomElement> Elements
    {
        get
        {
            return 
                _element.Attributes().Select(x => (IDomElement)new XmlDomAttribute(x))
                    .Concat(_element.Elements().Select(x => new XmlDomElement(x)))
                    .Concat(
                        !_element.HasElements && !string.IsNullOrEmpty(_element.Value)
                            ? new[] { new XmlDomText(_element.Value) }
                            : Enumerable.Empty<IDomElement>());
        }
    }
    
    public Property CreateProperty()
    {
        var property = new Property(_element.Name);
        
        property.AddPotentialPropertyDefinitions(
            BclClass.GetLegalClassesFromValue(_element.Value)
                .Select(bclClass =>
                    new PropertyDefinition(bclClass, _element.Name)
                    {
                        Attributes = new List<AttributeProxy>{ AttributeProxy.XmlElement(_element.Name.ToString()) }
                    }));
        
        var userDefinedClassPropertyDefinition =
            new PropertyDefinition(new UserDefinedClass(_element.Name.ToString()), _element.Name)
            {
                Attributes = new List<AttributeProxy>{ AttributeProxy.XmlElement(_element.Name.ToString()) }
            };
        
        if (HasElements)
        {
            property.PrependPotentialPropertyDefinition(userDefinedClassPropertyDefinition);
        }
        else
        {
            property.AppendPotentialPropertyDefinition(userDefinedClassPropertyDefinition);
        }
        
        return property;
    }
}

public class XmlDomAttribute : IDomElement
{
    private readonly XAttribute _attribute;

    public XmlDomAttribute(XAttribute attribute)
    {
        _attribute = attribute;
    }
    
    public bool HasElements
    {
        get { return false; }
    }
    
    public string Value
    {
        get { return _attribute.Value; }
    }
    
    public string Name
    {
        get { return _attribute.Name.ToString(); }
    }
    
    public IEnumerable<IDomElement> Elements
    {
        get
        {
            yield break;
        }
    }
    
    public Property CreateProperty()
    {
        var property = new Property(_attribute.Name);
        property.AddPotentialPropertyDefinitions(
            BclClass.GetLegalClassesFromValue(_attribute.Value)
                .Select(bclClass =>
                    new PropertyDefinition(bclClass, _attribute.Name)
                    {
                        Attributes = new List<AttributeProxy>{ AttributeProxy.XmlAttribute(_attribute.Name.ToString()) }
                    }));
        return property;
    }
}

public class XmlDomText : IDomElement
{
    private readonly string _value;

    public XmlDomText(string value)
    {
        _value = value;
    }
    
    public bool HasElements
    {
        get { return false; }
    }
    
    public string Value
    {
        get { return _value; }
    }
    
    public string Name
    {
        get { return "Value"; }
    }
    
    public IEnumerable<IDomElement> Elements
    {
        get
        {
            yield break;
        }
    }
    
    public Property CreateProperty()
    {
        var property = new Property(XName.Get(Name));
        
        property.AddPotentialPropertyDefinitions(
            BclClass.GetLegalClassesFromValue(_value)
                .Select(bclClass =>
                    new PropertyDefinition(bclClass, Name)
                    {
                        Attributes = new List<AttributeProxy>{ AttributeProxy.XmlText() }
                    }));
        
        return property;
    }
}

public interface IClassRepository
{
    IEnumerable<UserDefinedClass> GetAll();
    void AddOrUpdate(UserDefinedClass @class);
    UserDefinedClass GetOrCreate(string typeName);
}

public class ClassRepository : IClassRepository
{
    private static readonly ConcurrentDictionary<string, UserDefinedClass> _classes
        = new ConcurrentDictionary<string, UserDefinedClass>();

    public IEnumerable<UserDefinedClass> GetAll()
    {
        return _classes.Values.OrderBy(x => x.Order);
    }
    
    public void AddOrUpdate(UserDefinedClass @class)
    {
        _classes.AddOrUpdate(
            @class.TypeName.Raw,
            typeName => @class,
            (typeName, potentialClass) => GetOrCreate(typeName).MergeWith(@class));
    }
    
    public UserDefinedClass GetOrCreate(string typeName)
    {
        return _classes.GetOrAdd(
            typeName,
            x => new UserDefinedClass(typeName));
    }
}

public abstract class Class
{
    public abstract string GeneratePropertyCode(string propertyName, Case classCase);
}

public class UserDefinedClass : Class
{
    private static int _orderSeed;

    private readonly Dictionary<string, Property> _properties = new Dictionary<string, Property>();
    private readonly IdentifierName _typeName;
    private readonly int _order;

    public UserDefinedClass(string typeName)
    {
        _typeName = new IdentifierName(typeName);
        _order = _orderSeed++;
    }

    public IdentifierName TypeName
    {
        get { return _typeName; }
    }
    
    public int Order
    {
        get { return _order; }
    }
    
    public IEnumerable<Property> Properties
    {
        get { return _properties.Values; }
    }
    
    public void AddProperty(Property property)
    {
        Property foundProperty;
        if (!_properties.TryGetValue(property.Name.Raw, out foundProperty))
        {
            _properties.Add(property.Name.Raw, property);
            return;
        }
        
        foundProperty.AddPotentialPropertyDefinitions(property.PotentialPropertyDefinitions);
    }
    
    public UserDefinedClass MergeWith(UserDefinedClass other)
    {
        if (TypeName != other.TypeName)
        {
            // We shouldn't hit this situation, but just in case, bail.
            return this;
        }
    
        foreach (var otherProperty in other.Properties)
        {
            AddProperty(otherProperty);
        }
        
        return this;
    }
    
    public string GenerateCSharpCode(Case classCase, Case propertyCase)
    {
        return string.Format(
@"    [XmlRoot(""{0}"")]
    public class {1}
    {{
{2}
    }}",
            _typeName.Raw,
            _typeName.FormatAs(classCase),
            string.Join("\r\n\r\n", Properties.Select(x => x.GeneratePropertyCode(classCase, propertyCase))));
    }
    
    public override string GeneratePropertyCode(string propertyName, Case classCase)
    {
        return string.Format("public {0} {1} {{ get; set; }}", _typeName.FormatAs(classCase), propertyName);
    }
    
    public override bool Equals(object other)
    {
        var otherUserDefinedClass = other as UserDefinedClass;
        if (otherUserDefinedClass == null)
        {
            return false;
        }
        
        return _typeName.Raw == otherUserDefinedClass._typeName.Raw;
    }
}

public class BclClass : Class
{
    private static readonly ConcurrentDictionary<Type, BclClass> _classes = new ConcurrentDictionary<Type, BclClass>();
    private readonly Type _type;
    private readonly string _typeName;

    public BclClass(Type type, string typeName)
    {
        _type = type;
        _typeName = typeName;
    }

    public override string GeneratePropertyCode(string propertyName, Case classCase)
    {
        return string.Format("public {0} {1} {{ get; set; }}", _typeName, propertyName);
    }
    
    public override bool Equals(object other)
    {
        var otherBclClass = other as BclClass;
        if (otherBclClass == null)
        {
            return false;
        }
        
        return _type == otherBclClass._type;
    }
    
    public Type Type { get { return _type; } }
    public string TypeName { get { return _typeName; } }
    
    public static BclClass String
    {
        get { return _classes.GetOrAdd(typeof(string), type => new BclClass(type, "string")); }
    }
    
    public static BclClass Boolean
    {
        get { return _classes.GetOrAdd(typeof(bool), type => new BclClass(type, "bool")); }
    }
    
    public static BclClass Int32
    {
        get { return _classes.GetOrAdd(typeof(int), type => new BclClass(type, "int")); }
    }
    
    public static BclClass Decimal
    {
        get { return _classes.GetOrAdd(typeof(decimal), type => new BclClass(type, "decimal")); }
    }
    
    public static BclClass DateTime
    {
        get { return _classes.GetOrAdd(typeof(DateTime), type => new BclClass(type, "DateTime")); }
    }
    
    public static BclClass Guid
    {
        get { return _classes.GetOrAdd(typeof(Guid), type => new BclClass(type, "Guid")); }
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
    
    public static IEnumerable<BclClass> GetLegalClassesFromValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            yield return String;
            yield break;
        }
        
        int tempInt;
        if (int.TryParse(value, out tempInt))
        {
            yield return Int32;
        }
        
        decimal tempDecimal;
        if (decimal.TryParse(value, out tempDecimal))
        {
            yield return Decimal;
        }
        
        bool tempBool;
        if (bool.TryParse(value, out tempBool))
        {
            yield return Boolean;
        }
        
        System.Guid tempGuid;
        if (System.Guid.TryParse(value, out tempGuid))
        {
            yield return Guid;
        }
        
        System.DateTime tempDateTime;
        if (System.DateTime.TryParse(value, out tempDateTime))
        {
            yield return DateTime;
        }
        
        yield return String;
    }
}

public class Property
{
    private readonly List<PropertyDefinition> _potentialPropertyDefinitions = new List<PropertyDefinition>();

    public Property(XName propertyName)
    {
        Name = new IdentifierName(propertyName.ToString());
    }

    public IdentifierName Name { get; set; }
    
    public IEnumerable<PropertyDefinition> PotentialPropertyDefinitions
    {
        get { return _potentialPropertyDefinitions; }
    }
    
    public void PrependPotentialPropertyDefinition(PropertyDefinition potentialPropertyDefinition)
    {
        if (_potentialPropertyDefinitions.Any(x => x.Class == potentialPropertyDefinition.Class))
        {
            return;
        }
        
        InsertPotentialPropertyDefinition(0, potentialPropertyDefinition);
    }
    
    public void AppendPotentialPropertyDefinition(PropertyDefinition potentialPropertyDefinition)
    {
        InsertPotentialPropertyDefinition(_potentialPropertyDefinitions.Count, potentialPropertyDefinition);
    }
    
    private void InsertPotentialPropertyDefinition(int index, PropertyDefinition potentialPropertyDefinition)
    {
        if (_potentialPropertyDefinitions.Any(x => Equals(x.Class, potentialPropertyDefinition.Class)))
        {
            return;
        }
        
        _potentialPropertyDefinitions.Insert(index, potentialPropertyDefinition);
    }
    
    public void AddPotentialPropertyDefinitions(IEnumerable<PropertyDefinition> otherPotentialPropertyDefinitions)
    {
        foreach (var otherPotentialPropertyDefinition in otherPotentialPropertyDefinitions)
        {
            AppendPotentialPropertyDefinition(otherPotentialPropertyDefinition);
        }
    }
    
    public string GeneratePropertyCode(Case classCase, Case propertyCase)
    {
        return PotentialPropertyDefinitions.First().GeneratePropertyCode(classCase, propertyCase);
    }
}

public class PropertyDefinition
{
    public PropertyDefinition(Class @class, XName propertyName)
    {
        Class = @class;
        Name = new IdentifierName(propertyName.ToString());
        Attributes = new List<AttributeProxy>();
    }

    public Class Class { get; set; }
    public IdentifierName Name { get; set; }
    public List<AttributeProxy> Attributes { get; set; }
    
    public string GeneratePropertyCode(Case classCase, Case propertyCase)
    {
        var sb = new StringBuilder();
        
        foreach (var attribute in Attributes)
        {
            sb.AppendLine(string.Format("        {0}", attribute.ToCode()));
        }
        
        sb.AppendFormat("        {0}", Class.GeneratePropertyCode(Name.FormatAs(propertyCase), classCase));
        
        return sb.ToString();
    }
}

public class ClassGenerator
{
    private readonly IClassRepository _repository;

    public ClassGenerator(IClassRepository repository)
    {
        _repository = repository;
    }
        
    public void Write(Case classCase, Case propertyCase, PropertyAttributes propertyAttributes, TextWriter writer)
    {
        writer.WriteLine("using System;");
        writer.WriteLine("using System.Xml.Serialization;");
        writer.WriteLine();
        writer.WriteLine("namespace YourNamespaceHere");
        writer.WriteLine("{");
    
        writer.WriteLine(
            string.Join(
                "\r\n\r\n",
                _repository.GetAll().Select(x => x.GenerateCSharpCode(classCase, propertyCase))));
        
        writer.WriteLine("}");
    }
}

[Flags]
public enum PropertyAttributes
{
    XmlSerializion,
    DataContract
}

public enum Case
{
    PascalCase,
    camelCase,
    snake_case
}

public class IdentifierName
{
    private readonly string _rawIdentifierName;
    private readonly IList<string> _words;
    
    private readonly Lazy<string> _pascalCase;
    private readonly Lazy<string> _camelCase;
    private readonly Lazy<string> _snakeCase;

    public IdentifierName(string rawIdentifierName)
    {
        _rawIdentifierName = rawIdentifierName;
        
        _words = 
            Regex.Split(
                Regex.Replace(
                    rawIdentifierName.Replace(' ', '_'),
                    @"([A-Z])",
                    match => "_" + match.Value.ToLowerInvariant()),
                @"(?:_(?=[a-zA-Z]))|(?:^_$)")
            .Select(x => x.Trim().Trim('_'))
            .Where(x => !string.IsNullOrEmpty(x) && x.Length > 0)
            .ToList();
        
        _pascalCase = new Lazy<string>(
            () =>
            _words.Aggregate(
                "",
                (acc, n) =>
                    acc
                    + char.ToUpper(n[0]).ToString()
                    + (n.Length > 1 ? n.Substring(1) : "")));
                    
        _camelCase = new Lazy<string>(
            () =>
            _words.First()
            + _words.Skip(1).Aggregate(
                "",
                (acc, n) =>
                acc
                + char.ToUpper(n[0]).ToString()
                + (n.Length > 1 ? n.Substring(1) : "")));
                
        _snakeCase = new Lazy<string>(
            () =>
            string.Join("_", _words));
    }
    
    public string Raw
    {
        get { return _rawIdentifierName; }
    }
    
    private IList<string> Words
    {
        get { return _words; }
    }
    
    public string PascalCase
    {
        get { return _pascalCase.Value; }
    }
    
    public string camelCase
    {
        get { return _camelCase.Value; }
    }
    
    public string snake_case
    {
        get { return _snakeCase.Value; }
    }
    
    public string FormatAs(Case propertyCase)
    {
        switch (propertyCase)
        {
            case Case.PascalCase:
                return PascalCase;
            case Case.camelCase:
                return camelCase;
            case Case.snake_case:
                return snake_case;
            default:
                throw new InvalidOperationException("Invalid value for Case: " + (int)propertyCase);
        }
    }
}

public class ClassDefinitions
{
    public List<UserDefinedClassProxy> Classes { get; set; }
    
    public static ClassDefinitions FromClasses(IEnumerable<UserDefinedClass> classes)
    {
        var classDefinitions = new ClassDefinitions();
        classDefinitions.Classes =
            classes.Select(x => UserDefinedClassProxy.FromUserDefinedClass(x)).ToList();
        return classDefinitions;
    }
    
    public IEnumerable<UserDefinedClass> ToClasses(IClassRepository classRepository)
    {
        return Classes.Select(x => x.ToUserDefinedClass(classRepository)).ToList();
    }
}

[XmlRoot("Class")]
[XmlInclude(typeof(UserDefinedClassProxy))]
[XmlInclude(typeof(BclClassProxy))]
public class ClassProxy
{
    public string TypeName { get; set; }
    
    public static ClassProxy FromClass(Class @class)
    {
        if (@class is UserDefinedClass)
        {
            return UserDefinedClassProxy.FromUserDefinedClass((UserDefinedClass)@class);
        }
        else
        {
            return BclClassProxy.FromBclClass((BclClass)@class);
        }
    }
    
    public Class ToClass(IClassRepository classRepository)
    {
        if (this is UserDefinedClassProxy)
        {
            return classRepository.GetOrCreate(TypeName);
        }
        else
        {
            return BclClassProxy.ToBclClass((BclClassProxy)this);
        }
    }
}

[XmlRoot("UserDefinedClass")]
public class UserDefinedClassProxy : ClassProxy
{
    public List<PropertyProxy> Properties { get; set; }
    
    public static UserDefinedClassProxy FromUserDefinedClass(UserDefinedClass userDefinedClass)
    {
        return new UserDefinedClassProxy
        {
            TypeName = userDefinedClass.TypeName.Raw,
            Properties = userDefinedClass.Properties.Select(x => PropertyProxy.FromProperty(x)).ToList()
        };
    }
    
    public UserDefinedClass ToUserDefinedClass(IClassRepository classRepository)
    {
        var userDefinedClass = classRepository.GetOrCreate(TypeName);
        
        foreach (var propertyProxy in Properties)
        {
            userDefinedClass.AddProperty(propertyProxy.ToProperty(classRepository));
        }
        
        return userDefinedClass;
    }
}

[XmlRoot("BclClass")]
public class BclClassProxy : ClassProxy
{
    public string TypeFullName { get; set; }
    
    public static BclClassProxy FromBclClass(BclClass bclClass)
    {
        return new BclClassProxy
        {
            TypeName = bclClass.TypeName,
            TypeFullName = bclClass.Type.FullName
        };
    }
    
    public static BclClass ToBclClass(BclClassProxy bclClassProxy)
    {
        return new BclClass(Type.GetType(bclClassProxy.TypeFullName), bclClassProxy.TypeName);
    }
}

[XmlRoot("Property")]
public class PropertyProxy
{
    public string Name { get; set; }
    public List<PropertyDefinitionProxy> PotentialPropertyDefinitions { get; set; }
    
    public static PropertyProxy FromProperty(Property property)
    {
        return new PropertyProxy
        {
            Name = property.Name.Raw,
            PotentialPropertyDefinitions = property.PotentialPropertyDefinitions.Select(x => PropertyDefinitionProxy.FromPropertyDefinition(x)).ToList()
        };
    }
    
    public Property ToProperty(IClassRepository classRepository)
    {
        var property = new Property(XName.Get(Name));
        property.AddPotentialPropertyDefinitions(PotentialPropertyDefinitions.Select(x => x.ToPropertyDefinition(classRepository)));
        return property;
    }
}

[XmlRoot("PropertyDefinition")]
public class PropertyDefinitionProxy
{
    public string Name { get; set; }
    public ClassProxy Class { get; set; }
    public List<AttributeProxy> Attributes { get; set; }
    
    public static PropertyDefinitionProxy FromPropertyDefinition(PropertyDefinition propertyDefinition)
    {
        return new PropertyDefinitionProxy
        {
            Name = propertyDefinition.Name.Raw,
            Class = ClassProxy.FromClass(propertyDefinition.Class),
            Attributes = new List<AttributeProxy>(propertyDefinition.Attributes)
        };
    }
    
    public PropertyDefinition ToPropertyDefinition(IClassRepository classRepository)
    {
        var @class = Class.ToClass(classRepository);
        var propertyDefinition = new PropertyDefinition(@class, XName.Get(Name));
        propertyDefinition.Attributes = new List<AttributeProxy>(Attributes);
        return propertyDefinition;
    }
}

[XmlRoot("Attribute")]
public class AttributeProxy
{
    public string AttributeTypeName { get; set; }
    public string ElementNameSetter { get; set; }
    
    public static AttributeProxy XmlElement(string elementName)
    {
        return new AttributeProxy { AttributeTypeName = "XmlElement", ElementNameSetter = elementName };
    }
    
    public static AttributeProxy XmlAttribute(string attributeName)
    {
        return new AttributeProxy { AttributeTypeName = "XmlAttribute", ElementNameSetter = attributeName };
    }
    
    public static AttributeProxy XmlText()
    {
        return new AttributeProxy { AttributeTypeName = "XmlText" };
    }
    
    public string ToCode()
    {
        return ElementNameSetter != null
            ? string.Format("[{0}(\"{1}\")]", AttributeTypeName, ElementNameSetter)
            : string.Format("[{0}]", AttributeTypeName);
    }
}
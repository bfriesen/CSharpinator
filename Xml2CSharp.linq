<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Xml.Serialization</Namespace>
</Query>

#define NONEST
void Main()
{
    var xml = @"
<FooThis>
  <BarThat>
    <BazOther BazValue=""123"" />
    <BoomIt>abc</BoomIt>
  </BarThat>
</FooThis>";
//    var xml = @"
//<foo_this>
//  <bar_that>
//    <baz_other baz_value=""123"" />
//    <boom_it>abc</boom_it>
//  </bar_that>
//</foo_this>";

    var xmlDocument = XDocument.Parse(xml);
    var domElement = new XmlDomElement(xmlDocument.Root);
    
    var classRepository = new ClassRepository();
    
    var domVisitor = new DomVisitor(classRepository);
    domVisitor.Visit(domElement);
    
    var classGenerator = new ClassGenerator(classRepository);
    classGenerator.Write(
		Case.PascalCase,
		Case.PascalCase,
        PropertyAttributes.XmlSerializer | PropertyAttributes.Json,
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
            
            foreach (var childElement in element.Elements())
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
    IEnumerable<IDomElement> Elements();
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
    
    public IEnumerable<IDomElement> Elements()
    {
        return _element.Attributes().Select(x => (IDomElement)new XmlDomAttribute(x))
            .Concat(_element.Elements().Select(x => new XmlDomElement(x)));
    }
    
    public Property CreateProperty()
    {
        var property = new Property(_element.Name);
        
		property.AddPotentialPropertyDefinitions(
			BclClass.GetLegalClassesFromValue(_element.Value)
				.Select(bclClass =>
					new PropertyDefinition(bclClass, _element.Name)
					{
						XmlElement = new XmlElementAttribute(_element.Name.ToString())
					}));
		
		var userDefinedClassPropertyDefinition =
			new PropertyDefinition(new UserDefinedClass(_element.Name.ToString()), _element.Name)
			{
				XmlElement = new XmlElementAttribute(_element.Name.ToString())
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
    
    public IEnumerable<IDomElement> Elements()
    {
        yield break;
    }
    
    public Property CreateProperty()
    {
        var property = new Property(_attribute.Name);
        property.AddPotentialPropertyDefinitions(
			BclClass.GetLegalClassesFromValue(_attribute.Value)
				.Select(bclClass =>
					new PropertyDefinition(bclClass, _attribute.Name)
					{
						XmlAttribute = new XmlAttributeAttribute(_attribute.Name.ToString())
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
        return _classes.Values;
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
    private readonly Dictionary<string, Property> _properties = new Dictionary<string, Property>();
    private readonly IdentifierName _typeName;

    public UserDefinedClass(string typeName)
    {
        _typeName = new IdentifierName(typeName);
    }

    public IdentifierName TypeName
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
@"[XmlRoot(""{0}"")]
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
}

public class BclClass : Class
{
    private static readonly ConcurrentDictionary<Type, BclClass> _classes = new ConcurrentDictionary<Type, BclClass>();
    private readonly Type _type;

    private BclClass(Type type)
    {
        _type = type;
    }

    public override string GeneratePropertyCode(string propertyName, Case classCase)
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
        if (_potentialPropertyDefinitions.Any(x => x.Class == potentialPropertyDefinition.Class))
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
    }

    public Class Class { get; set; }
    public IdentifierName Name { get; set; }
    public XmlElementAttribute XmlElement { get; set; }
    public XmlAttributeAttribute XmlAttribute { get; set; }
    public XmlTextAttribute XmlText { get; set; }
    
    public string GeneratePropertyCode(Case classCase, Case propertyCase)
    {
        var sb = new StringBuilder();
        
        if (XmlElement != null)
        {
            sb.AppendLine(string.Format("        [XmlElement(\"{0}\")]", Name.Raw));
        }
        
        if (XmlAttribute != null)
        {
            sb.AppendLine(string.Format("        [XmlAttribute(\"{0}\")]", Name.Raw));
        }
        
        if (XmlText != null)
        {
            sb.AppendLine("        [XmlText]");
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
		writer.Write(
			string.Join(
				"\r\n\r\n",
				_repository.GetAll().Select(x => x.GenerateCSharpCode(classCase, propertyCase))));
    }
}

[Flags]
public enum PropertyAttributes
{
    XmlSerializer,
    Json
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
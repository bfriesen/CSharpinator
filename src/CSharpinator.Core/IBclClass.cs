namespace CSharpinator
{
    public interface IBclClass : IClass
    {
        string TypeName { get; }
        string TypeAlias { get; }
        bool IsNullable { get; }
        bool IsLegalStringValue(string value);
        bool IsLegalObjectValue(object value);
    }
}
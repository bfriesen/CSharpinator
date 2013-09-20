namespace CSharpifier
{
    public interface IBclClass : IClass
    {
        string TypeName { get; }
        string TypeAlias { get; }
        bool IsNullable { get; }
        bool IsLegalValue(string value);
    }
}
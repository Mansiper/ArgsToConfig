namespace ArgsToConfig.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsObjectAttribute : Attribute
{
    private string Name { get; }
    public ArgsObjectAttribute(string name) =>
        Name = name;

    internal string GetName => Name;
}
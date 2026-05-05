namespace ArgsToConfig.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsValueAttribute : Attribute
{
    private string Name { get; }
    public ArgsValueAttribute(string name) =>
        Name = name;

    internal string GetValue => Name;
}
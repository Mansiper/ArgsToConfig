namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that starting from the argument with the specified name, the fields of the nested object will be populated.
/// </summary>
/// <remarks>Works with object types only.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsObjectAttribute : Attribute
{
    private readonly string name;
    public ArgsObjectAttribute(string name) =>
        this.name = name;

    internal string GetName => name;
}
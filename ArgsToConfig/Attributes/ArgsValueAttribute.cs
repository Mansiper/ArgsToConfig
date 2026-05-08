namespace ArgsToConfig.Attributes;

/// <summary>
/// Specifies the name of the enum member as it appears in the command-line arguments.
/// </summary>
/// <remarks>Works with enum members only.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsValueAttribute : Attribute
{
    private readonly string name;

    public string? Description { get; set; }

    public ArgsValueAttribute(string name) =>
        this.name = name;

    internal string[] GetValues => name.Split('|');
}
namespace ArgsToConfig.Attributes;

/// <summary>
/// Maps the argument value to an enum member using the values defined by <see cref="ArgsValueAttribute"/> on enum members.
/// Multiple argument names can be specified by separating them with <c>|</c>.
/// </summary>
/// <remarks>Works only with enums.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsEnumAttribute : Attribute
{
    private readonly string? name;

    public bool Optional { get; set; }
    public string? DefaultValue { get; set; }
    public string? Description { get; set; }
    public string? EnvVar { get; set; }

    public ArgsEnumAttribute() { }

    public ArgsEnumAttribute(string name) =>
        this.name = name;

    internal string[]? GetNames => name?.Split('|');
}
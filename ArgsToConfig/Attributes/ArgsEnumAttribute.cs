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
    private readonly bool optional;

    public string? DefaultValue { get; set; }
    public string? Description { get; set; }

    public ArgsEnumAttribute() { }

    public ArgsEnumAttribute(string name, bool optional = false) =>
        (this.name, this.optional) = (name, optional);

    internal string[]? GetNames => name?.Split('|');
    internal bool GetOptional => optional;
}
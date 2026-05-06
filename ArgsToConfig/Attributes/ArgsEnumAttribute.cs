namespace ArgsToConfig.Attributes;

/// <summary>
/// Sets the value of the enum to the enum member
/// </summary>
/// <remarks>Works only with enums.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsEnumAttribute : Attribute
{
    private string? Name { get; }
    private bool Optional { get; }

    public string? DefaultValue { get; set; }

    public ArgsEnumAttribute() { }

    public ArgsEnumAttribute(string name, bool optional = false) =>
        (Name, Optional) = (name, optional);

    internal string[]? GetNames => Name?.Split('|');
    internal bool GetOptional => Optional;
}
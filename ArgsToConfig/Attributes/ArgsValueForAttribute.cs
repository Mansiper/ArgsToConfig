namespace ArgsToConfig.Attributes;

/// <summary>
/// The value of the parameter with the name specified in the attribute.
/// </summary>
/// <remarks>Works with any type, but string by default. Otherwise, tries to convert the value to the property type.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsValueForAttribute : Attribute
{
    private string Name { get; }
    private bool Optional { get; }

    public string DefaultValue { get; set; }

    public ArgsValueForAttribute(string name, bool optional = false) =>
        (Name, Optional) = (name, optional);

    internal string[] GetNames => Name.Split('|');
    internal bool GetOptional => Optional;
}
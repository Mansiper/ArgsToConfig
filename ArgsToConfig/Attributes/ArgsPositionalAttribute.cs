namespace ArgsToConfig.Attributes;

/// <summary>
/// Specifies the position (zero-based) of a positional argument for the field or property it is applied to.
/// Positional arguments are values without named identifiers. Without this attribute, such fields are assigned values in the order they appear in the class.
/// Use this attribute to define a custom order among positional fields.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsPositionalAttribute : Attribute
{
    private readonly int position;
    public ArgsPositionalAttribute(int position) => 
        this.position = position;

    internal int GetPosition => position;
}
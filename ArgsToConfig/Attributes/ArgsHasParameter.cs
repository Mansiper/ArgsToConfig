namespace ArgsToConfig.Attributes;

/// <summary>
/// Assigns <see langword="true"/> to the field or property if the argument with the specified name was provided; otherwise, the default value is kept.
/// Multiple argument names can be specified by separating them with <c>|</c>.
/// </summary>
/// <remarks>Works with boolean type only.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsHasParameterAttribute : Attribute
{
    private readonly string name;
    private readonly int position;

    public ArgsHasParameterAttribute(string name, int position = -1) => 
        (this.name, this.position) = (name, position);

    internal string[] GetNames => name.Split('|');
    internal int GetPosition => position;
}
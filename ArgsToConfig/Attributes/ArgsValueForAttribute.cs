namespace ArgsToConfig.Attributes;

/// <summary>
/// Specifies the name of a name-value argument. The value of that argument will be assigned to the field or property it is applied to.
/// Multiple argument names can be specified by separating them with <c>|</c>.
/// Works with any primitive type. Also works with tuple types when used together with <see cref="ArgsTupleAttribute"/>.
/// If <see cref="Optional"/> is <see langword="true"/> and the argument is not present, the default value (not <see langword="null"/>) is used instead.
/// The default value can be overridden via <see cref="DefaultValue"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsValueForAttribute : Attribute
{
    private readonly string name;

    public bool Optional { get; set; }
    public string? DefaultValue { get; set; }
    public string? Description { get; set; }
    public string? EnvVar { get; set; }

    public ArgsValueForAttribute(string name) =>
        this.name = name;

    internal string[] GetNames => name.Split('|');
}
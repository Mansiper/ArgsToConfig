namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to can only be assigned a value if all fields with the names specified in the attribute parameters are not <see langword="null"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsIfSetAttribute : Attribute
{
    private readonly string[] fields;

    public string? Description { get; set; }

    public ArgsIfSetAttribute(params string[] fields) => 
        this.fields = fields;

    internal string[] GetFields => fields;
}
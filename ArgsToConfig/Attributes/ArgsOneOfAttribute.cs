namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that only one of the specified fields and the field or property it is applied to can have a value at a time.
/// It is recommended to apply this attribute to the last of the listed fields.
/// </summary>
/// <remarks>All of these fields must be nullable.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsOneOfAttribute : Attribute
{
    private readonly string[] fields;
    public ArgsOneOfAttribute(params string[] fields) => 
        this.fields = fields;

    internal string[] GetFields => fields;
}
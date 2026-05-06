namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to must be assigned a value only after all the fields specified in the attribute parameters have been assigned.
/// If the user attempts to modify any of the specified fields after the attributed field or property has been assigned a value, an exception will be thrown.
/// The specified fields cannot be changed after the value is assigned to the field or property with this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsAfterAttribute : Attribute
{
    private readonly string[] fields;
    public string? Description { get; set; }

    public ArgsAfterAttribute(params string[] fields) => 
        this.fields = fields;

    internal string[] GetFields => fields;
}
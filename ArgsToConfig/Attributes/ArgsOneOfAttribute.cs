namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that only one of the specified fields can have a value at a time.
/// Must be applied to the class, not to individual properties.
/// Can be applied multiple times to define independent mutual-exclusion groups.
/// </summary>
/// <remarks>All listed fields must be nullable.</remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ArgsOneOfAttribute : Attribute
{
    private readonly string[] fields;
    
    public string? Description { get; set; }

    public ArgsOneOfAttribute(params string[] fields) => 
        this.fields = fields;

    internal string[] GetFields => fields;
}
namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that only one of the specified fields can have a value at a time.
/// </summary>
/// <remarks>All of these fields must be nullable.</remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ArgsOneOfAttribute : Attribute
{
    private readonly string[] fields;
    
    public string? Description { get; set; }

    public ArgsOneOfAttribute(params string[] fields) => 
        this.fields = fields;

    internal string[] GetFields => fields;
}
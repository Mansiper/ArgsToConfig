namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the decorated property or one of the decorated properties must be set while the others must not be set
/// </summary>
/// <remarks>All the decorated properties must be nullable. Apply it to the last of the fields</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsOneOfAttribute : Attribute
{
    private readonly string[] fields;
    public ArgsOneOfAttribute(params string[] fields) => 
        this.fields = fields;

    internal string[] GetFields => fields;
}
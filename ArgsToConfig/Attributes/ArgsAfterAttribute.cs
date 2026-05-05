namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the property it is applied to should be mapped from the argument that comes after all the properties specified in the attribute parameters.
/// No other optional properties should have values before it. This is useful for mapping arguments that come after a certain set of other arguments, ensuring that the mapping is done in the correct order.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsAfterAttribute : Attribute
{
    private readonly string[] fields;

    public ArgsAfterAttribute(params string[] fields) => 
        this.fields = fields;

    internal string[] GetFields => fields;
}
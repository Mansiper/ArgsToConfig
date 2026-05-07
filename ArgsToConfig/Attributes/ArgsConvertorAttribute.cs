namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to should be converted using the specified convertor type when parsing command-line arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsConvertorAttribute : Attribute
{
    private readonly Type convertorType;
    public string? Description { get; set; }
    
    public ArgsConvertorAttribute(Type convertorType) => 
        this.convertorType = convertorType;

    internal Type GetConvertorType => convertorType;
}
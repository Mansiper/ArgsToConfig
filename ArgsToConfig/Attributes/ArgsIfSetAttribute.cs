namespace ArgsToConfig.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsIfSetAttribute : Attribute
{
    private string[] Fields { get; }
    public ArgsIfSetAttribute(params string[] fields) => 
        Fields = fields;

    internal string[] GetFields => Fields;
}
namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the argument value will be split by the specified dividers and each part will be assigned to the corresponding element of the tuple.
/// </summary>
/// <remarks>Works with tuple types only (e.g. <c>(string, int)</c>).</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsTupleAttribute : Attribute
{
    private readonly string[] dividers;

    public string? Description { get; set; }

    public ArgsTupleAttribute(params string[] dividers) =>
        this.dividers = dividers;

    internal string[] GetDividers => dividers;
}
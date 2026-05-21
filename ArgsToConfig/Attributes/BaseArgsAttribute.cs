namespace ArgsToConfig.Attributes;

public abstract class BaseArgsAttribute : Attribute
{
    /// <summary>Gets or sets an optional human-readable description shown in help output.</summary>
    public string? Description { get; set; }
}
namespace ArgsToConfig.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsValueForBoolAttribute : Attribute
{
    private string TrueName { get; }
    private string FalseName { get; }

    public ArgsValueForBoolAttribute(string trueName, string falseName) =>
        (TrueName, FalseName) = (trueName, falseName);

    internal string[] GetTrueNames => TrueName.Split('|');
    internal string[] GetFalseNames => FalseName.Split('|');
}
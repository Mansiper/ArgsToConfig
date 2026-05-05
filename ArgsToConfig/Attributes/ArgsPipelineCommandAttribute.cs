namespace ArgsToConfig.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ArgsPipelineCommandAttribute : Attribute
{
    private string Name { get; }
    public ArgsPipelineCommandAttribute(string name) => 
        Name = name;

    internal string GetName => Name;
}
namespace ArgsToConfig.Attributes;

/// <summary>
/// Specifies the argument name from which the command starts in the command line for a pipeline command collection.
/// Applied to a class that implements the interface of a command collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ArgsPipelineCommandAttribute : Attribute
{
    private readonly string name;
    public ArgsPipelineCommandAttribute(string name) => 
        this.name = name;

    internal string GetName => name;
}
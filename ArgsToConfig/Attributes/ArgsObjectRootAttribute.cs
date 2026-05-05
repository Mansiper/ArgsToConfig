namespace ArgsToConfig.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ArgsObjectRootAttribute : Attribute
{
    private string Name { get; }
    public ArgsObjectRootAttribute(string name) => 
        Name = name;

    internal string GetName => Name;
}
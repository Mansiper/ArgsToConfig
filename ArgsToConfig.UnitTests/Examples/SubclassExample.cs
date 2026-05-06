using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// app connect - u user - p pass run
internal class SubclassExample
{
    [ArgsObject("connect")]
    public SubclassConnection Connect { get; set; } = null!;
    [ArgsHasParameter("run")]
    public bool? Run { get; set; }
}

internal class SubclassConnection
{
    [ArgsValueFor("-u")]
    public string User { get; set; } = null!;
    [ArgsValueFor("-p")]
    public string Pass { get; set; } = null!;
}

// wrong examples

internal class SubclassWithRunExample
{
    [ArgsObject("connect")]
    public SubclassConnectionWithRun Connect { get; set; } = null!;  
    [ArgsHasParameter("run")]
    public bool? Run { get; set; }
}

internal class SubclassConnectionWithRun
{
    [ArgsValueFor("-u")]
    public string User { get; set; } = null!;
    [ArgsValueFor("-p")]
    public string Pass { get; set; } = null!;
    [ArgsHasParameter("run")]
    public bool? Run { get; set; }
}
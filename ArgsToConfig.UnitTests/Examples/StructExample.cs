using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// app build --config Release --output bin

internal class StructExample
{
    [ArgsObject("build")]
    public StructBuildOptions Build { get; set; }
    [ArgsHasParameter("--verbose")]
    public bool? Verbose { get; set; }
}

internal struct StructBuildOptions
{
    [ArgsValueFor("--config")]
    public string Config { get; set; }
    [ArgsValueFor("--output")]
    public string Output { get; set; }
}
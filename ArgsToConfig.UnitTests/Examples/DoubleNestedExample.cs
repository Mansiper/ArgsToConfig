using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// app server start --host localhost --port 8080

internal class DoubleNestedExample
{
    [ArgsObject("server")]
    public DoubleNestedServer? Server { get; set; }
}

internal class DoubleNestedServer
{
    [ArgsObject("start")]
    public DoubleNestedStartOptions? Start { get; set; }
}

internal class DoubleNestedStartOptions
{
    [ArgsValueFor("--host")]
    public string Host { get; set; } = null!;
    [ArgsValueFor("--port")]
    public int Port { get; set; }
}
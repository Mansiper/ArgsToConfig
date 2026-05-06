using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// deploy [--fast] [--config <file>] <version>
internal class ArgsAfterExample
{
    [ArgsHasParameter("--fast")]
    public bool? Fast { get; set; }

    [ArgsValueFor("--config")]
    public string? Config { get; set; }

    [ArgsAfter(nameof(Fast))]
    public string? Version { get; set; }
}

// copy <source> <destination> [--tag <tag>]
internal class ArgsAfterMultipleExample
{
    public string? Source { get; set; }

    public string? Destination { get; set; }

    [ArgsAfter(nameof(Source), nameof(Destination))]
    public string? Tag { get; set; }
}

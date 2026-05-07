using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

public class AcceptFromAmongExample
{
    [ArgsValueFor("--format")]
    [ArgsAcceptFromAmong("jpg", "png", "gif")]
    public string? FileExtension { get; set; }

    [ArgsValueFor("--formats")]
    [ArgsAcceptFromAmong("jpg", "png", "gif")]
    public string[]? FileExtensions { get; set; }
}
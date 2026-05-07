using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// exec [--file <path> | --url <url>]
[ArgsOneOf(nameof(File), nameof(Url))]
internal class ArgsOneOfSingleExample
{
    [ArgsValueFor("--file")]
    public string? File { get; set; }

    [ArgsValueFor("--url")]
    public string? Url { get; set; }

    [ArgsValueFor("--output")]
    public string? Output { get; set; }
}

// exec [--file <path> | --url <url>] [--zip | --tar]
[ArgsOneOf(nameof(File), nameof(Url))]
[ArgsOneOf(nameof(Zip), nameof(Tar))]
internal class ArgsOneOfMultipleExample
{
    [ArgsValueFor("--file")]
    public string? File { get; set; }

    [ArgsValueFor("--url")]
    public string? Url { get; set; }

    [ArgsValueFor("--zip")]
    public string? Zip { get; set; }

    [ArgsValueFor("--tar")]
    public string? Tar { get; set; }
}

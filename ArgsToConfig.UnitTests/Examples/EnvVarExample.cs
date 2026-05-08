using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

internal class EnvVarExample
{
    [ArgsValueFor("--output", EnvVar = "APP_OUTPUT")]
    public string? Output { get; set; }

    [ArgsValueFor("--count", EnvVar = "APP_COUNT")]
    public int? Count { get; set; }

    [ArgsHasParameter("--verbose", EnvVar = "APP_VERBOSE")]
    public bool Verbose { get; set; }

    [ArgsEnum("--format", EnvVar = "APP_FORMAT")]
    public EnvVarFormat? Format { get; set; }
}

internal enum EnvVarFormat
{
    [ArgsValue("json")] Json,
    [ArgsValue("xml")] Xml,
    [ArgsValue("csv")] Csv,
}

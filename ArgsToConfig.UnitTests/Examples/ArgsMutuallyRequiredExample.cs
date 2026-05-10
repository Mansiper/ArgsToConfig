using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// exec [--user <name> --password <pass>]  (both must be set together or neither)
[ArgsMutuallyRequired(nameof(User), nameof(Password))]
internal class ArgsMutuallyRequiredSingleExample
{
    [ArgsValueFor("--user")]
    public string? User { get; set; }

    [ArgsValueFor("--password")]
    public string? Password { get; set; }

    [ArgsValueFor("--output")]
    public string? Output { get; set; }
}

// exec [--user <name> --password <pass>] [--host <h> --port <p>]
[ArgsMutuallyRequired(nameof(User), nameof(Password))]
[ArgsMutuallyRequired(nameof(Host), nameof(Port))]
internal class ArgsMutuallyRequiredMultipleExample
{
    [ArgsValueFor("--user")]
    public string? User { get; set; }

    [ArgsValueFor("--password")]
    public string? Password { get; set; }

    [ArgsValueFor("--host")]
    public string? Host { get; set; }

    [ArgsValueFor("--port")]
    public string? Port { get; set; }
}

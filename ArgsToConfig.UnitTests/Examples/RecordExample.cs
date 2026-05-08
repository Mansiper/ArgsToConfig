using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// app deploy --env production --tag v1.0 --dry-run

internal class RecordExample
{
    [ArgsObject("deploy")]
    public RecordDeployOptions? Deploy { get; set; }
    [ArgsHasParameter("--dry-run")]
    public bool? DryRun { get; set; }
}

internal record RecordDeployOptions
{
    [ArgsValueFor("--env")]
    public string Env { get; set; } = null!;
    [ArgsValueFor("--tag")]
    public string Tag { get; set; } = null!;
}
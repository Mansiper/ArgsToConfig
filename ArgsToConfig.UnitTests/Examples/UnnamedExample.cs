using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// mv [-x] old_path [-y] new_path [-z]
internal class UnnamedExample
{
    public string? OldPath { get; set; }
    public string? NewPath { get; set; }
    [ArgsHasParameter("-x")]
    public bool? X { get; set; }
    [ArgsHasParameter("-y")]
    public bool? Y { get; set; }
    [ArgsHasParameter("-z")]
    public bool? Z { get; set; }
}

internal class UnnamedPosExample
{
    [ArgsPositional(1)]
    public string? NewPath { get; set; }
    [ArgsPositional(0)]
    public string? OldPath { get; set; }
    [ArgsHasParameter("-x")]
    public bool? X { get; set; }
    [ArgsHasParameter("-y")]
    public bool? Y { get; set; }
    [ArgsHasParameter("-z")]
    public bool? Z { get; set; }
}

// wrong examples

internal class UnnamedSamePosExample
{
    [ArgsPositional(0)]
    public string? NewPath { get; set; }
    [ArgsPositional(0)]
    public string? OldPath { get; set; }
    [ArgsHasParameter("-x")]
    public bool? X { get; set; }
    [ArgsHasParameter("-y")]
    public bool? Y { get; set; }
    [ArgsHasParameter("-z")]
    public bool? Z { get; set; }
}

internal class UnnamedNoZeroPosExample
{
    [ArgsPositional(2)]
    public string? OldPath { get; set; }

    [ArgsPositional(1)]
    public string? NewPath { get; set; }

    [ArgsHasParameter("-x")]
    public bool? X { get; set; }
    [ArgsHasParameter("-y")]
    public bool? Y { get; set; }
    [ArgsHasParameter("-z")]
    public bool? Z { get; set; }
}

internal class UnnamedMissedPosExample
{
    [ArgsPositional(0)]
    public string? OldPath { get; set; }

    [ArgsPositional(2)]
    public string? NewPath { get; set; }

    [ArgsHasParameter("-x")]
    public bool? X { get; set; }
    [ArgsHasParameter("-y")]
    public bool? Y { get; set; }
    [ArgsHasParameter("-z")]
    public bool? Z { get; set; }
}
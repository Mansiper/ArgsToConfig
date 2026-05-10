using ArgsToConfig.Attributes;
// ReSharper disable InconsistentNaming

namespace ArgsToConfig.UnitTests.Examples;

/*
app run
    commandA -a -x --opt1 val1
    commandA -a -y --opt1 val1 --opt2 val2
    commandA -b -z --opt1 val1 --opt2 val2 --opt3
    commandB -r all
    commandB -l partial
    commandB -r half first
    commandC --sub1
    commandC --sub2 --sub2opt val
    commandC --sub3 --opt
    commandC --sub4 --opt
app start
app quit
*/

internal class ComplexPipelineExample
{
    [ArgsEnum]
    public AppCommand? AppCommand { get; set; }

    [ArgsPipeline]
    [ArgsIfSet(nameof(AppCommand))]
    public ISimplePipelineCommand[]? Commands { get; set; }
}

internal interface ISimplePipelineCommand;

// ── Enums ────────────────────────────────────────────────────────────────────

internal enum AppCommand
{
    [ArgsEnumValue("run")]
    Run,
    [ArgsEnumValue("start")]
    Start,
    [ArgsEnumValue("quit")]
    Quit,
}

internal enum CommandAMode
{
    [ArgsEnumValue("-x")]
    X,
    [ArgsEnumValue("-y")]
    Y,
    [ArgsEnumValue("-z")]
    Z,
}

internal enum CommandBTarget
{
    [ArgsEnumValue("all")]
    All,
    [ArgsEnumValue("partial")]
    Partial,
    [ArgsEnumValue("half")]
    Half,
}

internal enum CommandCSub
{
    [ArgsEnumValue("--sub1")]
    Sub1,
    [ArgsEnumValue("--sub2")]
    Sub2,
    [ArgsEnumValue("--sub3")]
    Sub3,
    [ArgsEnumValue("--sub4")]
    Sub4,
}

// ── Pipeline commands ─────────────────────────────────────────────────────────

[ArgsPipelineCommand("commandA")]
[ArgsOneOf(nameof(X), nameof(Y), nameof(Z))]
internal class SimpleCommandA : ISimplePipelineCommand
{
    [ArgsValueForBool("-a", "-b")]
    public bool? ModeA { get; set; }

    [ArgsObject("-x")]
    public CommandAXInfo? X { get; set; }
    [ArgsObject("-y")]
    public CommandAYInfo? Y { get; set; }
    [ArgsObject("-z")]
    public CommandAZInfo? Z { get; set; }
}

internal class CommandAXInfo
{
    [ArgsValueFor("--opt1")]
    public string? Opt1 { get; set; }
}

internal class CommandAYInfo
{
    [ArgsValueFor("--opt1")]
    public string? Opt1 { get; set; }
    [ArgsValueFor("--opt2")]
    public string? Opt2 { get; set; }
}

internal class CommandAZInfo
{
    [ArgsValueFor("--opt1")]
    public string? Opt1 { get; set; }
    [ArgsValueFor("--opt2")]
    public string? Opt2 { get; set; }
    [ArgsHasParameter("--opt3")]
    public bool? Opt3 { get; set; }
}

[ArgsPipelineCommand("commandB")]
internal class SimpleCommandB : ISimplePipelineCommand
{
    [ArgsValueForBool("-r", "-l")]
    public bool? ForR { get; set; }

    [ArgsEnum]
    public CommandBTarget? Target { get; set; }

    [ArgsPositional(0)]
    public string? Extra { get; set; }
}

[ArgsPipelineCommand("commandC")]
internal class SimpleCommandC : ISimplePipelineCommand
{
    [ArgsEnum]
    public CommandCSub? Sub { get; set; }

    [ArgsValueFor("--sub2opt")]
    public string? Sub2Opt { get; set; }

    [ArgsHasParameter("--opt")]
    public bool? Opt { get; set; }
}

using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

/*
exec pipeline
    pull [--fetch] [--force]
    commit [-m "message"]
    push [--force]
  run [--non-stop]
*/

internal class PipelineExample
{
    [ArgsHasParameter("pipeline")]
    public bool? Pipeline { get; set; }
    [ArgsPipeline]
    public IPipelineCommand[]? Commands { get; set; }
    [ArgsHasParameter("run")]
    public bool? Run { get; set; }
    [ArgsHasParameter("--non-stop")]
    public bool? NonStop { get; set; }
}

internal interface IPipelineCommand;

[ArgsPipelineCommand("pull")]
internal class PullCommand : IPipelineCommand
{
    [ArgsHasParameter("--fetch")]
    public bool? Fetch { get; set; }
    [ArgsHasParameter("--force")]
    public bool? Force { get; set; }
}

[ArgsPipelineCommand("commit")]
internal class CommitCommand : IPipelineCommand
{
    [ArgsValueFor("-m")]
    public string? Message { get; set; }
}

[ArgsPipelineCommand("push")]
internal class PushCommand : IPipelineCommand
{
    [ArgsHasParameter("--force")]
    public bool? Force { get; set; }
}

// wrong examples

internal interface IPipelineDuplicateCommand;

[ArgsPipelineCommand("pull")]
internal class DuplicatePullCommand : IPipelineDuplicateCommand { }

[ArgsPipelineCommand("pull")]
internal class DuplicatePullCommand2 : IPipelineDuplicateCommand { }

internal class DuplicatePipelineExample
{
    [ArgsPipeline]
    public IPipelineDuplicateCommand[]? Commands { get; set; }
}

internal interface IPipelineConflictCommand;

[ArgsPipelineCommand("run")]
internal class RunCommand : IPipelineConflictCommand { }

internal class ConflictingPipelineExample
{
    [ArgsHasParameter("run")]
    public bool? Run { get; set; }
    [ArgsPipeline]
    public IPipelineConflictCommand[]? Commands { get; set; }
}

internal interface IPipelineArgConflictCommand;

[ArgsPipelineCommand("pull")]
internal class PullWithCommitArgCommand : IPipelineArgConflictCommand
{
    [ArgsHasParameter("commit")]
    public bool? Commit { get; set; }
}

[ArgsPipelineCommand("commit")]
internal class CommitArgConflictCommand : IPipelineArgConflictCommand { }

internal class ArgConflictPipelineExample
{
    [ArgsPipeline]
    public IPipelineArgConflictCommand[]? Commands { get; set; }
}
using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

public class PathConstraintExample
{
    [ArgsValueFor("--file")]
    [ArgsExistingOnlyFile]
    public string? FilePath { get; set; }

    [ArgsValueFor("--dir")]
    [ArgsExistingOnlyDirectory]
    public string? DirPath { get; set; }

    [ArgsValueFor("--name")]
    [ArgsLegalFileNamesOnly]
    public string? FileName { get; set; }
}
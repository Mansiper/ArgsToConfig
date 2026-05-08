using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// app level1 level2 level3 level4 level5 --value somevalue

internal class FiveNestedExample
{
    [ArgsObject("level1")]
    public FiveNestedLevel1? Level1 { get; set; }
}

internal class FiveNestedLevel1
{
    [ArgsObject("level2")]
    public FiveNestedLevel2? Level2 { get; set; }
}

internal class FiveNestedLevel2
{
    [ArgsObject("level3")]
    public FiveNestedLevel3? Level3 { get; set; }
}

internal class FiveNestedLevel3
{
    [ArgsObject("level4")]
    public FiveNestedLevel4? Level4 { get; set; }
}

internal class FiveNestedLevel4
{
    [ArgsObject("level5")]
    public FiveNestedLevel5? Level5 { get; set; }
}

internal class FiveNestedLevel5
{
    [ArgsValueFor("--value")]
    public string Value { get; set; } = null!;
}

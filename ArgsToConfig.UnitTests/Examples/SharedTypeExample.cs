using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// exec -a yes -b no
internal class SharedEnumExample
{
    [ArgsEnum("-a")]
    public SharedEnumYesNo? A { get; set; }

    [ArgsEnum("-b")]
    public SharedEnumYesNo? B { get; set; }
}

internal enum SharedEnumYesNo
{
    [ArgsValue("yes")]
    Yes,
    [ArgsValue("no")]
    No,
}

// exec source -u user -p pass target -u user -p pass
internal class SharedClassExample
{
    [ArgsObject("source")]
    public SharedEndpoint? Source { get; set; }

    [ArgsObject("target")]
    public SharedEndpoint? Target { get; set; }
}

internal class SharedEndpoint
{
    [ArgsValueFor("-u")]
    public string? User { get; set; }

    [ArgsValueFor("-p")]
    public string? Pass { get; set; }
}

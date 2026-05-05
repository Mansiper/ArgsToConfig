using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

/*
exec 
    cp11 -x
    cp12
    cp13
    cp21 -a
    cp22 -b
*/

internal class PipelineMultipleExample
{
    [ArgsPipeline]
    public IPipelineMultipleCommand1[]? Commands1 { get; set; }
    [ArgsPipeline]
    public List<IPipelineMultipleCommand2>? Commands2 { get; set; }
}

internal interface IPipelineMultipleCommand1;
internal interface IPipelineMultipleCommand2;

[ArgsPipelineCommand("cp11")]
internal class ComandP11 : IPipelineMultipleCommand1
{
    [ArgsHasParameter("-x")]
    public bool? X { get; set; }
}

[ArgsPipelineCommand("cp12")]
internal class ComandP12 : IPipelineMultipleCommand1;

[ArgsPipelineCommand("cp13")]
internal class ComandP13 : IPipelineMultipleCommand1;

[ArgsPipelineCommand("cp21")]
internal class ComandP21 : IPipelineMultipleCommand2
{
    [ArgsHasParameter("-a")]
    public bool? A { get; set; }
}

[ArgsPipelineCommand("cp22")]
internal class ComandP22 : IPipelineMultipleCommand2
{
    [ArgsHasParameter("-b")]
    public bool? B { get; set; }
}

// --------------

internal interface IPipelineMultiple3Command1;
internal interface IPipelineMultiple3Command2;
internal interface IPipelineMultiple3Command3;

[ArgsPipelineCommand("c3p11")]
internal class Comand3P11 : IPipelineMultiple3Command1
{
    [ArgsHasParameter("-x")]
    public bool? X { get; set; }
}

[ArgsPipelineCommand("c3p12")]
internal class Comand3P12 : IPipelineMultiple3Command1;

[ArgsPipelineCommand("c3p21")]
internal class Comand3P21 : IPipelineMultiple3Command2;

[ArgsPipelineCommand("c3p31")]
internal class Comand3P31 : IPipelineMultiple3Command3
{
    [ArgsHasParameter("-z")]
    public bool? Z { get; set; }
}

internal class Pipeline3Example
{
    [ArgsPipeline]
    public IPipelineMultiple3Command1[]? Commands1 { get; set; }
    [ArgsPipeline]
    public IPipelineMultiple3Command2[]? Commands2 { get; set; }
    [ArgsPipeline]
    public IPipelineMultiple3Command3[]? Commands3 { get; set; }
}

// wrong examples

// ------------

internal interface IPipelineMultipleDup1;
internal interface IPipelineMultipleDup2;

[ArgsPipelineCommand("cpdup")]
internal class ComandPDup1 : IPipelineMultipleDup1;

[ArgsPipelineCommand("cpdup")]
internal class ComandPDup2 : IPipelineMultipleDup2;

internal class DuplicateCrossMultiplePipelineExample
{
    [ArgsPipeline]
    public IPipelineMultipleDup1[]? Commands1 { get; set; }
    [ArgsPipeline]
    public IPipelineMultipleDup2[]? Commands2 { get; set; }
}

// ------------

internal class SameInterfaceMultiplePipelineExample
{
    [ArgsPipeline]
    public IPipelineMultipleCommand1[]? Commands1 { get; set; }
    [ArgsPipeline]
    public IPipelineMultipleCommand1[]? Commands2 { get; set; }
}

// ------------

internal class MixedOrderMultiplePipelineExample
{
    [ArgsPipeline]
    public IPipelineMultipleCommand1[]? Commands1 { get; set; }
    [ArgsPipeline]
    public List<IPipelineMultipleCommand2>? Commands2 { get; set; }
}
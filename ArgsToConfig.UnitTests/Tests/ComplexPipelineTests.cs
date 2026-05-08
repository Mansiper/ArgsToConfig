using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class ComplexPipelineTests
{
    [Test]
    public void ComplexPipeline_ShouldSucceed()
    {
        // Arrange
        string[] args =
        [
            "run",
            "commandA", "-a", "-x", "--opt1", "val1",
            "commandA", "-a", "-y", "--opt1", "val1", "--opt2", "val2",
            "commandA", "-b", "-z", "--opt1", "val1", "--opt2", "val2", "--opt3",
            "commandB", "-r", "all",
            "commandB", "-l", "partial",
            "commandB", "-r", "half", "first",
            "commandC", "--sub1",
            "commandC", "--sub2", "--sub2opt", "val",
            "commandC", "--sub3", "--opt",
            "commandC", "--sub4", "--opt",
        ];

        var expected = new ComplexPipelineExample
        {
            AppCommand = AppCommand.Run,
            Commands =
            [
                new SimpleCommandA { ModeA = true, X = new() { Opt1 = "val1" } },
                new SimpleCommandA { ModeA = true, Y = new() { Opt1 = "val1", Opt2 = "val2" } },
                new SimpleCommandA { ModeA = false, Z = new() { Opt1 = "val1", Opt2 = "val2", Opt3 = true } },
                new SimpleCommandB { ForR = true, Target = CommandBTarget.All },
                new SimpleCommandB { ForR = false, Target = CommandBTarget.Partial },
                new SimpleCommandB { ForR = true, Target = CommandBTarget.Half, Extra = "first" },
                new SimpleCommandC { Sub = CommandCSub.Sub1 },
                new SimpleCommandC { Sub = CommandCSub.Sub2, Sub2Opt = "val" },
                new SimpleCommandC { Sub = CommandCSub.Sub3, Opt = true },
                new SimpleCommandC { Sub = CommandCSub.Sub4, Opt = true },
            ],
        };

        // Act
        var (result, errors, _) = ArgumentsReader.ToObject<ComplexPipelineExample>(args);

        // Assert
        errors.Should().BeNull();
        result.Should().BeEquivalentTo(expected);
    }
}
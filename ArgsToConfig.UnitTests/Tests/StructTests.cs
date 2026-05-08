using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class StructTests
{
    [Test]
    public void Build_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "build", "--config", "Release", "--output", "bin" };

        var expected = new StructExample
        {
            Build = new StructBuildOptions { Config = "Release", Output = "bin" },
            Verbose = null
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<StructExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Build_WithVerbose_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--verbose", "build", "--config", "Debug", "--output", "out" };

        var expected = new StructExample
        {
            Build = new StructBuildOptions { Config = "Debug", Output = "out" },
            Verbose = true
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<StructExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}
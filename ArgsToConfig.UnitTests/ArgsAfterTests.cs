using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

// deploy [--fast] [--config <file>] <version>

[TestFixture]
public class ArgsAfterTests
{
    [Test]
    public void ShouldSucceed_WhenFastFlagIsSetBeforeVersion()
    {
        // Arrange
        var args = new[] { "--fast", "1.0.0" };

        var expected = new ArgsAfterExample
        {
            Fast = true,
            Version = "1.0.0",
        };

        // Act
        var result = ArgumentsReader.ToObject<ArgsAfterExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ShouldSucceed_WhenFastFlagAndConfigAreSetBeforeVersion()
    {
        // Arrange
        var args = new[] { "--fast", "--config", "file.json", "2.3.1" };

        var expected = new ArgsAfterExample
        {
            Fast = true,
            Config = "file.json",
            Version = "2.3.1",
        };

        // Act
        var result = ArgumentsReader.ToObject<ArgsAfterExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ShouldSucceed_WhenOnlyConfigIsSetWithoutFast()
    {
        // Arrange
        var args = new[] { "--config", "file.json", "1.0.0" };

        var expected = new ArgsAfterExample
        {
            Fast = null,
            Config = "file.json",
            Version = null,
        };

        // Act
        var result = ArgumentsReader.ToObject<ArgsAfterExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ShouldNotAssignVersion_WhenFastFlagIsMissing()
    {
        // Arrange - no --fast flag, so Version prerequisite is not met
        var args = new[] { "1.0.0" };

        var expected = new ArgsAfterExample
        {
            Fast = null,
            Version = null,
        };

        // Act
        var result = ArgumentsReader.ToObject<ArgsAfterExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ShouldNotAssignVersion_WhenFastFlagAppearsAfterVersion()
    {
        // Arrange - wrong args order: --fast comes after the positional version value
        var args = new[] { "--config", "file.json", "1.0.0", "--fast" };

        var expected = new ArgsAfterExample
        {
            Fast = true,
            Config = "file.json",
            Version = null,
        };

        // Act
        var result = ArgumentsReader.ToObject<ArgsAfterExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ShouldSucceed_WhenAllMultiplePrerequisitesAreSet()
    {
        // Arrange - Source and Destination are positional, Tag follows them
        var args = new[] { "src", "dst", "v1" };

        var expected = new ArgsAfterMultipleExample
        {
            Source = "src",
            Destination = "dst",
            Tag = "v1",
        };

        // Act
        var result = ArgumentsReader.ToObject<ArgsAfterMultipleExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ShouldAssignTag_WhenOnlyOnePrerequisiteIsSet()
    {
        // Arrange - Destination is optional, so Source alone is enough for Tag to be assigned
        var args = new[] { "src", "v1" };

        var expected = new ArgsAfterMultipleExample
        {
            Source = "src",
            Destination = null,
            Tag = "v1",
        };

        // Act
        var result = ArgumentsReader.ToObject<ArgsAfterMultipleExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}
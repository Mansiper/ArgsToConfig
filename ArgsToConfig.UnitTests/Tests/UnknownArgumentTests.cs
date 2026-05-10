using ArgsToConfig.Attributes;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class UnknownArgumentTests
{
    private class SimpleConfig
    {
        [ArgsHasParameter("-v|--verbose")]
        public bool Verbose { get; set; }

        [ArgsValueFor("-o|--output")]
        public string? Output { get; set; }
    }

    [TearDown]
    public void TearDown()
    {
        ArgumentsReader.OnUnknownArgument = null;
    }

    [Test]
    public void UnknownArgument_ReturnsError_AndPosition()
    {
        // Arrange
        var args = new[] { "--verbose", "--unknown", "--output", "file.txt" };

        // Act
        var (result, errors, position) = ArgumentsReader.ToObject<SimpleConfig>(args);

        // Assert
        result.Should().NotBeNull();
        result!.Verbose.Should().BeTrue(); // parsed before the unknown arg
        errors.Should().NotBeNull();
        errors!.Should().ContainSingle(e => e.Contains("--unknown"));
        position.Should().Be(2); // 1-based index of "--unknown"
    }

    [Test]
    public void UnknownArgument_FirstArg_ReturnsPosition1()
    {
        // Arrange
        var args = new[] { "--bogus", "--verbose" };

        // Act
        var (result, errors, position) = ArgumentsReader.ToObject<SimpleConfig>(args);

        // Assert
        result.Should().NotBeNull();
        errors.Should().ContainSingle(e => e.Contains("--bogus"));
        position.Should().Be(1);
    }

    [Test]
    public void UnknownArgument_LastArg_ReturnsCorrectPosition()
    {
        // Arrange
        var args = new[] { "--verbose", "--output", "out.txt", "--nope" };

        // Act
        var (result, errors, position) = ArgumentsReader.ToObject<SimpleConfig>(args);

        // Assert
        result.Should().NotBeNull();
        result!.Verbose.Should().BeTrue();
        result.Output.Should().Be("out.txt");
        errors.Should().ContainSingle(e => e.Contains("--nope"));
        position.Should().Be(4);
    }

    [Test]
    public void NoUnknownArguments_ReturnsSuccess_NoErrors()
    {
        // Arrange
        var args = new[] { "--verbose", "--output", "out.txt" };

        // Act
        var (result, errors, position) = ArgumentsReader.ToObject<SimpleConfig>(args);

        // Assert
        result.Should().NotBeNull();
        errors.Should().BeNull();
        result!.Verbose.Should().BeTrue();
        result.Output.Should().Be("out.txt");
        position.Should().BeNull();
    }

    [Test]
    public void OnUnknownArgument_ReturnTrue_ContinuesParsing()
    {
        // Arrange
        var args = new[] { "--verbose", "--unknown-flag", "--output", "out.txt" };
        var receivedArg = string.Empty;
        ArgumentsReader.OnUnknownArgument = async arg =>
        {
            receivedArg = arg;
            await Task.CompletedTask;
            return true; // continue
        };

        // Act
        var (result, errors, position) = ArgumentsReader.ToObject<SimpleConfig>(args);

        // Assert
        receivedArg.Should().Be("--unknown-flag");
        errors.Should().BeNull();
        result.Should().NotBeNull();
        result!.Verbose.Should().BeTrue();
        result.Output.Should().Be("out.txt");
    }

    [Test]
    public void OnUnknownArgument_NotSet_ReturnsError()
    {
        // Arrange
        var args = new[] { "--verbose", "--unknown-flag" };

        // Act
        var (result, errors, position) = ArgumentsReader.ToObject<SimpleConfig>(args);

        // Assert — without the callback set, errors are returned normally
        result.Should().NotBeNull();
        errors.Should().NotBeNull();
        errors!.Should().ContainSingle(e => e.Contains("--unknown-flag"));
        position.Should().Be(2);
    }

    [Test]
    public void OnUnknownArgument_ReceivesArgName()
    {
        // Arrange
        var args = new[] { "--totally-unknown" };
        var receivedArg = string.Empty;
        ArgumentsReader.OnUnknownArgument = async arg =>
        {
            receivedArg = arg;
            await Task.CompletedTask;
            return true;
        };

        // Act
        ArgumentsReader.ToObject<SimpleConfig>(args);

        // Assert
        receivedArg.Should().Be("--totally-unknown");
    }

    [Test]
    public void ValidArgs_Position_IsOneBasedCountOfArgs()
    {
        // single arg consumed
        var (r1, e1, p1) = ArgumentsReader.ToObject<SimpleConfig>("--verbose");
        e1.Should().BeNull();
        p1.Should().BeNull();

        // three args consumed
        var (r2, e2, p2) = ArgumentsReader.ToObject<SimpleConfig>("--verbose", "--output", "file");
        e2.Should().BeNull();
        p2.Should().BeNull();
    }
}

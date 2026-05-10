using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class ArgsMutuallyRequiredTests
{
    [Test]
    public void MutuallyRequired_BothSet_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--user", "alice", "--password", "secret" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<ArgsMutuallyRequiredSingleExample>(args);

        // Assert
        result!.User.Should().Be("alice");
        result.Password.Should().Be("secret");
    }

    [Test]
    public void MutuallyRequired_NeitherSet_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--output", "out.txt" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<ArgsMutuallyRequiredSingleExample>(args);

        // Assert
        result!.User.Should().BeNull();
        result.Password.Should().BeNull();
        result.Output.Should().Be("out.txt");
    }

    [Test]
    public void MutuallyRequired_OnlyFirstSet_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--user", "alice" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ArgsMutuallyRequiredSingleExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(1);
    }

    [Test]
    public void MutuallyRequired_OnlySecondSet_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--password", "secret" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ArgsMutuallyRequiredSingleExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(1);
    }

    [Test]
    public void MultipleGroups_FirstGroupIncomplete_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--user", "alice", "--host", "localhost", "--port", "5432" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ArgsMutuallyRequiredMultipleExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(1);
    }

    [Test]
    public void MultipleGroups_SecondGroupIncomplete_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--user", "alice", "--password", "secret", "--host", "localhost" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ArgsMutuallyRequiredMultipleExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(5);
    }

    [Test]
    public void MultipleGroups_AllSet_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--user", "alice", "--password", "secret", "--host", "localhost", "--port", "5432" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<ArgsMutuallyRequiredMultipleExample>(args);

        // Assert
        result!.User.Should().Be("alice");
        result.Password.Should().Be("secret");
        result.Host.Should().Be("localhost");
        result.Port.Should().Be("5432");
    }

    [Test]
    public void MultipleGroups_NoneSet_ShouldSucceed()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<ArgsMutuallyRequiredMultipleExample>(args);

        // Assert
        result!.User.Should().BeNull();
        result.Password.Should().BeNull();
        result.Host.Should().BeNull();
        result.Port.Should().BeNull();
    }
}
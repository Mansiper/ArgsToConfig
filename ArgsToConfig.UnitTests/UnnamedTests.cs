using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

// mv [-x] old_path [-y] new_path [-z]

[TestFixture]
public class UnnamedTests
{
    [Test]
    public void ShouldSucceed_WithOrderOfFields()
    {
        // Arrange
        var args = new[] { "old", "new" };

        var expected = new UnnamedExample
        {
            OldPath = "old",
            NewPath = "new",
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<UnnamedExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ShouldSucceed_WithOrderViaAtrributes()
    {
        // Arrange
        var args = new[] { "-x", "old", "-y", "new", "-z" };

        var expected = new UnnamedPosExample
        {
            OldPath = "old",
            NewPath = "new",
            X = true,
            Y = true,
            Z = true,
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<UnnamedExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ShouldFail_WhenSamePosition()
    {
        // Arrange
        var args = new[] { "old", "new" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<UnnamedSamePosExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().BeNull();
    }

    [Test]
    public void ShouldFail_WhenNoZeroPosition()
    {
        // Arrange
        var args = new[] { "old", "new" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<UnnamedNoZeroPosExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().BeNull();
    }

    [Test]
    public void ShouldSucceed_WhenMissingPositionalArgs()
    {
        // Arrange
        var args = new[] { "old", "new" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<UnnamedMissedPosExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().BeNull();
    }
}
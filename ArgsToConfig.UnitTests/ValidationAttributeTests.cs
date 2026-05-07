using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

[TestFixture]
public class ValidationAttributeTests
{
    [Test]
    public void AllValid_ShouldSucceed()
    {
        // Arrange
        var args = new[]
        {
            "--name", "Alice",
            "--email", "alice@example.com",
            "--phone", "+1-800-555-0100",
            "--count", "42",
            "--tag", "short",
            "--code", "ABC"
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<ValidationExample>(args);

        // Assert
        result!.Name.Should().Be("Alice");
        result.Email.Should().Be("alice@example.com");
        result.Phone.Should().Be("+1-800-555-0100");
        result.Count.Should().Be(42);
        result.Tag.Should().Be("short");
        result.Code.Should().Be("ABC");
    }

    [Test]
    public void Required_Missing_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--count", "5" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ValidationExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().BeNull();
    }

    [Test]
    public void InvalidEmail_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--name", "Alice", "--email", "not-an-email" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ValidationExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().BeNull();
    }

    [Test]
    public void InvalidPhone_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--name", "Alice", "--phone", "notaphone!!!" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ValidationExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().BeNull();
    }

    [Test]
    public void RangeExceeded_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--name", "Alice", "--count", "200" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ValidationExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().BeNull();
    }

    [Test]
    public void MaxLengthExceeded_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--name", "Alice", "--tag", "this-tag-is-way-too-long" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ValidationExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().BeNull();
    }

    [Test]
    public void RegexMismatch_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--name", "Alice", "--code", "invalid123" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ValidationExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().BeNull();
    }

    [Test]
    public void MultipleViolations_ShouldThrowWithAllMessages()
    {
        // Arrange – email invalid + range exceeded
        var args = new[] { "--name", "Alice", "--email", "bad", "--count", "999" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ValidationExample>(args);

        // Assert
        errors.Should().NotBeNull();
        errors.Length.Should().BeGreaterThan(1);
        position.Should().BeNull();
    }
}

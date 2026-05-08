using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class ConvertorTests
{
    [Test]
    public void IPv4_Valid_ShouldParseToIntArray()
    {
        // Arrange
        var args = new[] { "--ip", "192.168.1.100" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<ConvertorExample>(args);

        // Assert
        result!.IpAddress.Should().Equal(192, 168, 1, 100);
    }

    [Test]
    public void IPv4_NotProvided_ShouldReturnNull()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<ConvertorExample>(args);

        // Assert
        result!.IpAddress.Should().BeNull();
    }

    [Test]
    public void IPv4_TooFewOctets_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--ip", "192.168.1" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ConvertorExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }
}
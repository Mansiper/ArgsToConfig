using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

[TestFixture]
public class ConvertorTests
{
    [Test]
    public void IPv4_Valid_ShouldParseToIntArray()
    {
        // Arrange
        var args = new[] { "--ip", "192.168.1.100" };

        // Act
        var result = ArgumentsReader.ToObject<ConvertorExample>(args);

        // Assert
        result.IpAddress.Should().Equal(192, 168, 1, 100);
    }

    [Test]
    public void IPv4_NotProvided_ShouldReturnNull()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var result = ArgumentsReader.ToObject<ConvertorExample>(args);

        // Assert
        result.IpAddress.Should().BeNull();
    }

    [Test]
    public void IPv4_TooFewOctets_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--ip", "192.168.1" };

        // Act
        var act = () => ArgumentsReader.ToObject<ConvertorExample>(args);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*not a valid IPv4 address*");
    }
}
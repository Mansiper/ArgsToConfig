using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class DoubleNestedTests
{
    [Test]
    public void ServerStart_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "server", "start", "--host", "localhost", "--port", "8080" };

        var expected = new DoubleNestedExample
        {
            Server = new DoubleNestedServer
            {
                Start = new DoubleNestedStartOptions { Host = "localhost", Port = 8080 }
            }
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<DoubleNestedExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Server_WithoutStart_ShouldFail()
    {
        // Arrange
        var args = new[] { "server", "--host", "localhost" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<DoubleNestedExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(1);
    }
}
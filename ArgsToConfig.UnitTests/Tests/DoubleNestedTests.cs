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

    [Test]
    public void FiveNestedLevels_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "level1", "level2", "level3", "level4", "level5", "--value", "somevalue" };

        var expected = new FiveNestedExample
        {
            Level1 = new FiveNestedLevel1
            {
                Level2 = new FiveNestedLevel2
                {
                    Level3 = new FiveNestedLevel3
                    {
                        Level4 = new FiveNestedLevel4
                        {
                            Level5 = new FiveNestedLevel5 { Value = "somevalue" }
                        }
                    }
                }
            }
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<FiveNestedExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}
using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class ArgsOneOfTests
{
    [Test]
    public void OneOf_OnlyFile_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--file", "data.csv" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<ArgsOneOfSingleExample>(args);

        // Assert
        result!.File.Should().Be("data.csv");
        result.Url.Should().BeNull();
    }

    [Test]
    public void OneOf_OnlyUrl_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--url", "https://example.com" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<ArgsOneOfSingleExample>(args);

        // Assert
        result!.Url.Should().Be("https://example.com");
        result.File.Should().BeNull();
    }

    [Test]
    public void OneOf_NeitherSet_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--output", "out.txt" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<ArgsOneOfSingleExample>(args);

        // Assert
        result!.File.Should().BeNull();
        result.Url.Should().BeNull();
        result.Output.Should().Be("out.txt");
    }

    [Test]
    public void OneOf_BothSet_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--file", "data.csv", "--url", "https://example.com" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ArgsOneOfSingleExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(3);
    }

    [Test]
    public void MultipleOneOf_FirstGroupConflict_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--file", "data.csv", "--url", "https://example.com", "--zip", "archive.zip" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ArgsOneOfMultipleExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(3);
    }

    [Test]
    public void MultipleOneOf_SecondGroupConflict_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--file", "data.csv", "--zip", "archive.zip", "--tar", "archive.tar" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ArgsOneOfMultipleExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(5);
    }

    [Test]
    public void MultipleOneOf_OneFromEachGroup_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--file", "data.csv", "--zip", "archive.zip" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<ArgsOneOfMultipleExample>(args);

        // Assert
        result!.File.Should().Be("data.csv");
        result.Url.Should().BeNull();
        result.Zip.Should().Be("archive.zip");
        result.Tar.Should().BeNull();
    }

    [Test]
    public void MultipleOneOf_NoneSet_ShouldSucceed()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<ArgsOneOfMultipleExample>(args);

        // Assert
        result!.File.Should().BeNull();
        result.Url.Should().BeNull();
        result.Zip.Should().BeNull();
        result.Tar.Should().BeNull();
    }
}
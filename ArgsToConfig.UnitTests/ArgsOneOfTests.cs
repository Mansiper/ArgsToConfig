using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

[TestFixture]
public class ArgsOneOfTests
{
    [Test]
    public void OneOf_OnlyFile_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--file", "data.csv" };

        // Act
        var result = ArgumentsReader.ToObject<ArgsOneOfSingleExample>(args);

        // Assert
        result.File.Should().Be("data.csv");
        result.Url.Should().BeNull();
    }

    [Test]
    public void OneOf_OnlyUrl_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--url", "https://example.com" };

        // Act
        var result = ArgumentsReader.ToObject<ArgsOneOfSingleExample>(args);

        // Assert
        result.Url.Should().Be("https://example.com");
        result.File.Should().BeNull();
    }

    [Test]
    public void OneOf_NeitherSet_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--output", "out.txt" };

        // Act
        var result = ArgumentsReader.ToObject<ArgsOneOfSingleExample>(args);

        // Assert
        result.File.Should().BeNull();
        result.Url.Should().BeNull();
        result.Output.Should().Be("out.txt");
    }

    [Test]
    public void OneOf_BothSet_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--file", "data.csv", "--url", "https://example.com" };

        // Act
        var act = () => ArgumentsReader.ToObject<ArgsOneOfSingleExample>(args);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*mutually exclusive*");
    }

    [Test]
    public void MultipleOneOf_FirstGroupConflict_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--file", "data.csv", "--url", "https://example.com", "--zip", "archive.zip" };

        // Act
        var act = () => ArgumentsReader.ToObject<ArgsOneOfMultipleExample>(args);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*mutually exclusive*");
    }

    [Test]
    public void MultipleOneOf_SecondGroupConflict_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--file", "data.csv", "--zip", "archive.zip", "--tar", "archive.tar" };

        // Act
        var act = () => ArgumentsReader.ToObject<ArgsOneOfMultipleExample>(args);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*mutually exclusive*");
    }

    [Test]
    public void MultipleOneOf_OneFromEachGroup_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--file", "data.csv", "--zip", "archive.zip" };

        // Act
        var result = ArgumentsReader.ToObject<ArgsOneOfMultipleExample>(args);

        // Assert
        result.File.Should().Be("data.csv");
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
        var result = ArgumentsReader.ToObject<ArgsOneOfMultipleExample>(args);

        // Assert
        result.File.Should().BeNull();
        result.Url.Should().BeNull();
        result.Zip.Should().BeNull();
        result.Tar.Should().BeNull();
    }
}
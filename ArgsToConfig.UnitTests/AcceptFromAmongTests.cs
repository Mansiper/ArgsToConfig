using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

[TestFixture]
public class AcceptFromAmongTests
{
    // ── Single value ──────────────────────────────────────────────────────────

    [Test]
    public void AcceptFromAmong_AcceptedValue_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--format", "jpg" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<AcceptFromAmongExample>(args);

        // Assert
        result!.FileExtension.Should().Be("jpg");
    }

    [Test]
    public void AcceptFromAmong_AcceptedValueCaseInsensitive_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--format", "jpg" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<AcceptFromAmongExample>(args);

        // Assert
        result!.FileExtension.Should().Be("jpg");
    }

    [Test]
    public void AcceptFromAmong_NotAcceptedValue_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--format", "tiff" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<AcceptFromAmongExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().BeNull();
    }

    [Test]
    public void AcceptFromAmong_NotProvided_ShouldReturnNull()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<AcceptFromAmongExample>(args);

        // Assert
        result!.FileExtension.Should().BeNull();
    }

    [TestCase("jpg")]
    [TestCase("png")]
    [TestCase("gif")]
    public void AcceptFromAmong_AllAcceptedValues_ShouldSucceed(string ext)
    {
        // Arrange
        var args = new[] { "--format", ext };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<AcceptFromAmongExample>(args);

        // Assert
        result!.FileExtension.Should().Be(ext);
    }

    // ── Collection values ─────────────────────────────────────────────────────

    [Test]
    public void AcceptFromAmong_MultipleAcceptedValues_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--formats", "jpg", "--formats", "gif" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<AcceptFromAmongExample>(args);

        // Assert
        result!.FileExtensions.Should().Equal("jpg", "gif");
    }

    [Test]
    public void AcceptFromAmong_CollectionWithNotAcceptedValue_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--formats", "jpg", "--formats", "bmp" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<AcceptFromAmongExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().BeNull();
    }
}

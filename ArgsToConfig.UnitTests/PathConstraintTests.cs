using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

[TestFixture]
public class PathConstraintTests
{
    private string existingFile = null!;
    private string existingDir = null!;

    [SetUp]
    public void SetUp()
    {
        existingFile = Path.GetTempFileName();
        existingDir = Path.GetTempPath();
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(existingFile))
            File.Delete(existingFile);
    }

    // ── ArgsExistingOnlyFile ──────────────────────────────────────────────────

    [Test]
    public void ExistingOnlyFile_ExistingFile_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--file", existingFile };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<PathConstraintExample>(args);

        // Assert
        result!.FilePath.Should().Be(existingFile);
    }

    [Test]
    public void ExistingOnlyFile_NonExistingFile_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--file", @"C:\this\does\not\exist.txt" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<PathConstraintExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void ExistingOnlyFile_NotProvided_ShouldReturnNull()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<PathConstraintExample>(args);

        // Assert
        result!.FilePath.Should().BeNull();
    }

    // ── ArgsExistingOnlyDirectory ─────────────────────────────────────────────

    [Test]
    public void ExistingOnlyDirectory_ExistingDir_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--dir", existingDir };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<PathConstraintExample>(args);

        // Assert
        result!.DirPath.Should().Be(existingDir);
    }

    [Test]
    public void ExistingOnlyDirectory_NonExistingDir_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--dir", @"C:\this\does\not\exist" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<PathConstraintExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void ExistingOnlyDirectory_NotProvided_ShouldReturnNull()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<PathConstraintExample>(args);

        // Assert
        result!.DirPath.Should().BeNull();
    }

    // ── ArgsLegalFileNamesOnly ────────────────────────────────────────────────

    [Test]
    public void LegalFileNamesOnly_LegalName_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--name", "my-report_2024.txt" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<PathConstraintExample>(args);

        // Assert
        result!.FileName.Should().Be("my-report_2024.txt");
    }

    [Test]
    public void LegalFileNamesOnly_IllegalName_ShouldThrow()
    {
        // Arrange
        var args = new[] { "--name", "bad|name?.txt" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<PathConstraintExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void LegalFileNamesOnly_NotProvided_ShouldReturnNull()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<PathConstraintExample>(args);

        // Assert
        result!.FileName.Should().BeNull();
    }
}
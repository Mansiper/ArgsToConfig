using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

/*
git commit [-a | --interactive | --patch] [-s] [-v] [-u[<mode>]] [--amend]
   [--dry-run] [--fixup [(amend|reword):<commit>]]
   [-F <file> | -m <msg>] [--reset-author] [--allow-empty]
   [--allow-empty-message] [--no-verify] [-e] [--author=<author>]
   [--date=<date>] [--cleanup=<mode>] [--[no-]status]
   [-i | -o] [--pathspec-from-file=<file> [--pathspec-file-nul]]
   [(--trailer <token>[(=|:)<value>])…​] [-S[<keyid>]]
   [--] [<pathspec>…​]
*/

[TestFixture]
public class GitCommitTests
{
    [Test]
    public void AllParams_ShouldSucceed()
    {
        // Arrange
        var args = new[]
        {
            "commit",
            "-a", "-s", "-v", "-u", "no", "--amend",
            "--dry-run", "--fixup", "amend:123456",
            "-F", "file.txt", "--reset-author", "--allow-empty",
            "--allow-empty-message", "--no-verify", "-e", "--author=\"John Doe\"",
            "--date=2024-06-01", "--cleanup=strip", "--status",
            "-i", "--pathspec-from-file=paths.txt", "--pathspec-file-nul",
            "--trailer", "token=value", "--trailer", "token2:value2", "-S", "keyid"
        };

        var expected = new GitCommitExample
        {
            IsCommit = true,
            CommitMode = CommitMode.A,
            SignOff = true,
            Verbose = true,
            UntrackedFiles = UntrackedFiles.No,
            Amend = true,
            DryRun = true,
            FixupCommit = "amend:123456",
            File = "file.txt",
            Message = null,
            ResetAuthor = true,
            AllowEmpty = true,
            AllowEmptyMessage = null,
            NoVerify = true,
            Edit = true,
            Author = "John Doe",
            Date = new DateTime(2024, 6, 1),
            Cleanup = CleanupMode.Strip,
            Status = true,
            IncludeOnly = IncludeOnly.Include,
            PathspecFromFile = "paths.txt",
            PathspecFileNul = true,
            Trailer = ["token=value", "token2:value2"],
            SignKeyId = "keyid"
        };

        // Act
        var result = ArgumentsReader.ToObject<GitCommitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void CombinedShortArgs_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "commit", "-am", "text" };
        
        var expected = new GitCommitExample
        {
            IsCommit = true,
            CommitMode = CommitMode.A,
            Message = "text"
        };
     
        // Act
        var result = ArgumentsReader.ToObject<GitCommitExample>(args);
        
        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void OrderOfArgs_ShouldNotMatter()
    {
        // Arrange
        var args = new[] { "commit", "-v", "-a" };
        
        var expected = new GitCommitExample
        {
            IsCommit = true,
            CommitMode = CommitMode.A,
            Verbose = true,
            Cleanup = CleanupMode.Default,
        };
     
        // Act
        var result = ArgumentsReader.ToObject<GitCommitExample>(args);
        
        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ConflictingArgs_ShouldFail()
    {
        // Arrange
        var args = new[] { "commit", "-a", "--interactive" };
        
        // Act
        Action act = () => ArgumentsReader.ToObject<GitCommitExample>(args);
        
        // Assert
        act.Should().Throw<ArgumentException>();    //todo: add message check
    }

    [Test]
    public void WrongEnumValue_ShouldFail()
    {
        // Arrange
        var args = new[] { "commit", "-u", "wrong" };
        
        // Act
        Action act = () => ArgumentsReader.ToObject<GitCommitExample>(args);
        
        // Assert
        act.Should().Throw<ArgumentException>();    //todo: add message check
    }

    [Test]
    public void MissingValue_ShouldFail()
    {
        // Arrange
        var args = new[] { "commit", "-F", "-e" };
        
        // Act
        Action act = () => ArgumentsReader.ToObject<GitCommitExample>(args);
        
        // Assert
        act.Should().Throw<ArgumentException>();    //todo: add message check
    }

    [Test]
    public void ValueStartingWithDash_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "commit", "-F", "\"-e\"" };
        
        var expected = new GitCommitExample
        {
            IsCommit = true,
            File = "-e",
            Cleanup = CleanupMode.Default,
        };
     
        // Act
        var result = ArgumentsReader.ToObject<GitCommitExample>(args);
        
        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void WrongDateFormat_ShouldFail()
    {
        // Arrange
        var args = new[] { "commit", "--date=wrong" };
        
        // Act
        Action act = () => ArgumentsReader.ToObject<GitCommitExample>(args);
        
        // Assert
        act.Should().Throw<ArgumentException>();    //todo: add message check
    }

    [Test]
    public void ConflictingStatus_ShouldFail()
    {
        // Arrange
        var args = new[] { "commit", "--no-status", "--status" };
        
        // Act
        Action act = () => ArgumentsReader.ToObject<GitCommitExample>(args);
        
        // Assert
        act.Should().Throw<ArgumentException>();    //todo: add message check
    }

    [Test]
    public void WrongArgumentName_ShouldFail()
    {
        // Arrange
        var args = new[] { "commit", "--not-status" };
        
        // Act
        Action act = () => ArgumentsReader.ToObject<GitCommitExample>(args);
        
        // Assert
        act.Should().Throw<ArgumentException>();    //todo: add message check
    }

    [Test]
    public void PathspecFileNulWithoutFromFile_ShouldFail()
    {
        // Arrange
        var args = new[] { "commit", "--pathspec-file-nul" };
        
        // Act
        Action act = () => ArgumentsReader.ToObject<GitCommitExample>(args);
        
        // Assert
        act.Should().Throw<ArgumentException>();    //todo: add message check
    }

    [Test]
    public void EndOfOptions_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "commit", "-a", "--", "--patch" };
        
        var expected = new GitCommitExample
        {
            IsCommit = true,
            CommitMode = CommitMode.A,
            Pathspec = ["--patch"],
            Cleanup = CleanupMode.Default,
        };
     
        // Act
        var result = ArgumentsReader.ToObject<GitCommitExample>(args);
        
        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}
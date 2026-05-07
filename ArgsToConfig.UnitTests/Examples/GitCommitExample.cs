using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

/*git commit [-a | --interactive | --patch] [-s] [-v] [-u[<mode>]] [--amend]
   [--dry-run] [--fixup [(amend|reword):<commit>]]
   [-F <file> | -m <msg>] [--reset-author] [--allow-empty]
   [--allow-empty-message] [--no-verify] [-e] [--author=<author>]
   [--date=<date>] [--cleanup=<mode>] [--[no-]status]
   [-i | -o] [--pathspec-from-file=<file> [--pathspec-file-nul]]
   [(--trailer <token>[(=|:)<value>])…​] [-S[<keyid>]]
   [--] [<pathspec>…​]*/
internal class GitCommitExample
{
    [ArgsHasParameter("commit", 0)]
    public bool IsCommit { get; set; }

    [ArgsEnum]
    public CommitMode? CommitMode { get; set; }

    [ArgsValueForBool("-s|--signoff", "--no-signoff")]
    public bool SignOff { get; set; }

    [ArgsHasParameter("-v|--verbose")]
    public bool Verbose { get; set; }

    [ArgsEnum("-u|--untracked-files", true, DefaultValue = "Normal")]
    public UntrackedFiles UntrackedFiles { get; set; }

    [ArgsHasParameter("--amend")]
    public bool Amend { get; set; }

    [ArgsHasParameter("--dry-run")]
    public bool DryRun { get; set; }

    [ArgsValueFor("--fixup")]
    [ArgsTuple(":")]
    public (string, string)? FixupCommit { get; set; }

    [ArgsValueFor("-F|--file", true)]
    public string? File { get; set; }

    [ArgsValueFor("-m|--message", true)]
    [ArgsOneOf(nameof(File))]
    public string? Message { get; set; }

    [ArgsHasParameter("--reset-author")]
    public bool ResetAuthor { get; set; }

    [ArgsHasParameter("--allow-empty")]
    public bool AllowEmpty { get; set; }

    [ArgsHasParameter("--allow-empty-message")]
    public string? AllowEmptyMessage { get; set; }

    [ArgsHasParameter("--no-verify")]
    public bool NoVerify { get; set; }
    
    [ArgsHasParameter("-e|--edit")]
    public bool Edit { get; set; }
    
    [ArgsValueFor("--author")]
    public string? Author { get; set; }

    [ArgsValueFor("--date")]
    public DateTime? Date { get; set; }

    [ArgsEnum("--cleanup", DefaultValue = "Default")]
    public CleanupMode Cleanup { get; set; }
    
    [ArgsValueForBool("--status", "--no-status")]
    public bool Status { get; set; }

    [ArgsEnum]
    public IncludeOnly? IncludeOnly { get; set; }

    [ArgsValueFor("--pathspec-from-file")]
    public string? PathspecFromFile { get; set; }

    [ArgsHasParameter("--pathspec-file-nul")]
    [ArgsIfSet(nameof(PathspecFromFile))]
    public bool PathspecFileNul { get; set; }

    [ArgsValueFor("--trailer")]
    [ArgsTuple("=", ":")]
    public List<(string, int)>? Trailer { get; set; }

    [ArgsValueFor("-S", true)]
    [ArgsIfSet(nameof(Trailer))]
    public string? SignKeyId { get; set; }

    [ArgsPathspec]
    public string[]? Pathspec { get; set; }
}

internal enum CommitMode
{
    [ArgsHasParameter("-a|--all")]
    A,
    [ArgsHasParameter("--interactive")]
    Interactive,
    [ArgsHasParameter("-p|--patch")]
    Patch
}

internal enum UntrackedFiles
{
    [ArgsValue("normal")]
    Normal,
    [ArgsValue("all")]
    All,
    [ArgsValue("no")]
    No,
}

internal enum CleanupMode
{
    Default,
    Strip,
    Whitespace,
    Verbatim,
    Scissors
}

internal enum IncludeOnly
{
    [ArgsHasParameter("-i")]
    Include,
    [ArgsHasParameter("-o")]
    Only
}
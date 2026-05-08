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
[ArgsOneOf(nameof(File), nameof(Message), Description = "Only one of --file or --message can be specified")]
internal class GitCommitExample
{
    [ArgsHasParameter("commit", 0, Description = "The 'commit' subcommand keyword")]
    public bool IsCommit { get; set; }

    [ArgsEnum(Description = "Commit mode: stage all tracked changes (-a/--all), interactively select changes (--interactive), or use patch mode (-p/--patch)")]
    public CommitMode? CommitMode { get; set; }

    [ArgsValueForBool("-s|--signoff", "--no-signoff", Description = "Add or remove a Signed-off-by trailer to the commit message")]
    public bool SignOff { get; set; }

    [ArgsHasParameter("-v|--verbose", Description = "Show unified diff between HEAD and what would be committed at the bottom of the commit message")]
    public bool Verbose { get; set; }

    [ArgsEnum("-u|--untracked-files", Optional = true, DefaultValue = "Normal", Description = "Show untracked files. Mode: normal, all, or no")]
    public UntrackedFiles UntrackedFiles { get; set; }

    [ArgsHasParameter("--amend", Description = "Replace the tip of the current branch by creating a new commit")]
    public bool Amend { get; set; }

    [ArgsHasParameter("--dry-run", Description = "Do not create a commit, but show a list of paths that would be committed")]
    public bool DryRun { get; set; }

    [ArgsValueFor("--fixup", Description = "Create a new commit which fixes a previous commit. Format: (amend|reword):<commit>")]
    [ArgsTuple(":")]
    public (string, string)? FixupCommit { get; set; }

    [ArgsValueFor("-F|--file", Optional = true, Description = "Take the commit message from the given file")]
    public string? File { get; set; }

    [ArgsValueFor("-m|--message", Optional = true, Description = "Use the given message as the commit message")]
    public string? Message { get; set; }

    [ArgsHasParameter("--reset-author", Description = "When amending, reset the author to the committer identity")]
    public bool ResetAuthor { get; set; }

    [ArgsHasParameter("--allow-empty", Description = "Allow recording a commit that has the exact same tree as its parent")]
    public bool AllowEmpty { get; set; }

    [ArgsHasParameter("--allow-empty-message", Description = "Allow a commit with an empty commit message")]
    public string? AllowEmptyMessage { get; set; }

    [ArgsHasParameter("--no-verify", Description = "Bypass the pre-commit and commit-msg hooks")]
    public bool NoVerify { get; set; }

    [ArgsHasParameter("-e|--edit", Description = "Open the commit message in an editor even when one is given via -m or -F")]
    public bool Edit { get; set; }

    [ArgsValueFor("--author", Description = "Override the commit author. Format: A U Thor <author@example.com>")]
    public string? Author { get; set; }

    [ArgsValueFor("--date", Description = "Override the author date used in the commit")]
    public DateTime? Date { get; set; }

    [ArgsEnum("--cleanup", DefaultValue = "Default", Description = "Determine how the commit message is cleaned up. Mode: default, strip, whitespace, verbatim, scissors")]
    public CleanupMode Cleanup { get; set; }

    [ArgsValueForBool("--status", "--no-status", Description = "Include or exclude the output of git-status in the commit message template")]
    public bool Status { get; set; }

    [ArgsEnum(Description = "Include (-i) or only commit (-o) specified files. Mutually exclusive.")]
    public IncludeOnly? IncludeOnly { get; set; }

    [ArgsValueFor("--pathspec-from-file", Description = "Read pathspec from the given file")]
    public string? PathspecFromFile { get; set; }

    [ArgsHasParameter("--pathspec-file-nul", Description = "NUL-terminate pathspec entries when using --pathspec-from-file")]
    [ArgsIfSet(nameof(PathspecFromFile))]
    public bool PathspecFileNul { get; set; }

    [ArgsValueFor("--trailer", Description = "Append a trailer token=value or token:value to the commit message. Can be repeated.")]
    [ArgsTuple("=", ":")]
    public List<(string, int)>? Trailer { get; set; }

    [ArgsValueFor("-S", Optional = true, Description = "GPG-sign the commit using the given key ID (requires --trailer)")]
    [ArgsIfSet(nameof(Trailer))]
    public string? SignKeyId { get; set; }

    [ArgsPathspec(Description = "Paths to limit the commit to. Must follow -- separator.")]
    public string[]? Pathspec { get; set; }
}

internal enum CommitMode
{
    [ArgsValue("-a|--all", Description = "Stage all modifications and deletions automatically")]
    A,
    [ArgsValue("--interactive", Description = "Interactively select which changes to include")]
    Interactive,
    [ArgsValue("-p|--patch", Description = "Use the interactive patch selection interface to choose hunks")]
    Patch
}

internal enum UntrackedFiles
{
    [ArgsValue("normal", Description = "Show untracked files and directories")]
    Normal,
    [ArgsValue("all", Description = "Also show individual files in untracked directories")]
    All,
    [ArgsValue("no", Description = "Show no untracked files")]
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
    [ArgsValue("-i", Description = "Include specified files in the commit")]
    Include,
    [ArgsValue("-o", Description = "Only commit specified files, ignoring staged content")]
    Only
}
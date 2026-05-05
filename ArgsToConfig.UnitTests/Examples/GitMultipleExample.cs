using System;
using System.Collections.Generic;
using System.Text;
using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

/*
git clone path
or
git commit [-m <msg>]
 */

internal class GitMultipleExample
{
    [ArgsObject]
    public GitMultipleClone? Clone { get; set; }

    [ArgsObject]
    public GitMultipleCommit? Commit { get; set; }
}

[ArgsObjectRoot("clone")]
internal class GitMultipleClone
{
    public string? Path { get; set; }
}

[ArgsObjectRoot("commit")]
internal class GitMultipleCommit
{
    [ArgsValueFor("-m|--message")]
    public string? Message { get; set; }
}
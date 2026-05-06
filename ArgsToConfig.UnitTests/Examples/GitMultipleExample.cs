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
    [ArgsObject("clone")]
    public GitMultipleClone? Clone { get; set; }

    [ArgsObject("commit")]
    public GitMultipleCommit? Commit { get; set; }
}

internal class GitMultipleClone
{
    public string? Path { get; set; }
}

internal class GitMultipleCommit
{
    [ArgsValueFor("-m|--message")]
    public string? Message { get; set; }
}
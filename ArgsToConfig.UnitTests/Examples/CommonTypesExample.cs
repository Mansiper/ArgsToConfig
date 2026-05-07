using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// exec --date 2024-06-15 --time 14:30:00 --span 01:30:00 --file notes.txt --url https://example.com --guid d3b07384-d9a5-4e7d-a8d0-1234567890ab --ip 192.168.1.1 --version 1.2.3.4
internal class CommonTypesExample
{
    [ArgsValueFor("--date")]
    public DateTime? Date { get; set; }

    [ArgsValueFor("--only-date")]
    public DateOnly? OnlyDate { get; set; }

    [ArgsValueFor("--time")]
    public TimeOnly? Time { get; set; }

    [ArgsValueFor("--span")]
    public TimeSpan? Span { get; set; }

    [ArgsValueFor("--file")]
    public FileInfo? File { get; set; }

    [ArgsValueFor("--url")]
    public Uri? Url { get; set; }

    [ArgsValueFor("--guid")]
    public Guid? Guid { get; set; }

    [ArgsValueFor("--ip")]
    public System.Net.IPAddress? Ip { get; set; }

    [ArgsValueFor("--version")]
    public Version? Version { get; set; }
}

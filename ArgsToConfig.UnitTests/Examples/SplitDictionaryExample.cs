using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// Dictionary: first divider splits key from value; remaining dividers split the value further.
//
// --define KEY=VALUE                  => Dictionary<string, string>   divider: "="
// --threshold cpu=90                  => Dictionary<string, int>      divider: "="
// --entry name=hello,42               => Dictionary<string, (string, int)> dividers: "=", ","
// --tags env=prod,staging             => Dictionary<string, string[]> dividers: "=", ","

internal class SplitDictionaryExample
{
    // Simple string → string dictionary
    [ArgsValueFor("--define")]
    [ArgsSplit("=")]
    public Dictionary<string, string>? Defines { get; set; }

    // String → int dictionary
    [ArgsValueFor("--threshold")]
    [ArgsSplit("=")]
    public Dictionary<string, int>? Thresholds { get; set; }

    // String → (string, int) tuple dictionary: first divider "=" splits key/value, second "," splits tuple parts
    [ArgsValueFor("--entry")]
    [ArgsSplit("=", ",")]
    public Dictionary<string, (string, int)>? Entries { get; set; }

    // String → string[] collection dictionary: first "=" splits key/value, second "," splits collection elements
    [ArgsValueFor("--tags")]
    [ArgsSplit("=", ",")]
    public Dictionary<string, string[]>? Tags { get; set; }
}
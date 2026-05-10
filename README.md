# CLI arguments to configuration

**ArgsToConfig** is a .NET library that maps CLI arguments directly onto a strongly-typed configuration class using attributes.  
No manual parsing loops — just decorate your properties and call `ArgumentsReader.ToObject<T>()`.

> 📦 NuGet: *link coming soon*  

*Breaking changes are still possible until the 1.0.0 release.*

---

## Installation

```
dotnet add package ArgsToConfig
```

---

## Quick start

```csharp
var (config, errors, position) = ArgumentsReader.ToObject<MyConfig>(args);
if (errors is not null)
{
    Console.Error.WriteLine(string.Join('\n', errors));
    if (position is not null)
        Console.Error.WriteLine($"Error at argument #{position}");
    return;
}
```

`ToObject<T>` returns a value tuple `(T? result, string[]? errors, int? position)`.  
On success `errors` is `null`; on failure `result` is `null`, `errors` contains the error messages, and `position` is the 1-based index of the offending argument (or `null` for validation errors). **No exceptions are thrown.**

Decorate the properties of `MyConfig` with the appropriate attributes (see below) and the library handles the rest.

> 💡 **Optional arguments** — if an argument is optional, simply declare the property as a nullable type (e.g. `string?`, `bool?`, `int?`). When the argument is absent, the property will remain `null`.

---

## Supported types

The following types are supported out-of-the-box for all value-accepting attributes (`[ArgsEnumValueFor]`, `[ArgsPositional]`, tuple elements, etc.):

`string`, `bool`, `int`, `double`, `float`, `long`, `decimal`, `char`,  
`DateTime`, `DateOnly`, `TimeOnly`, `TimeSpan`,  
`Guid`, `Uri`, `Version`, `System.Net.IPAddress`,  
`FileInfo`, `DirectoryInfo`,  
any `enum`, any `IConvertible`

For any other type use `[ArgsConvertor]` with a custom `IArgsConvertor` implementation.

---

## Converting back to arguments

```csharp
// object → string[]
string[] argv = ArgumentsReader.ToArgs(config);

// object → synopsis string  (e.g. "-v --output <outputpath> [--format <format>]")
string synopsis = ArgumentsReader.ToArgsString<MyConfig>();
```

`ToArgs<T>` reconstructs a `string[]` from a populated config object that can be passed back to any CLI parser.  
`ToArgsString<T>()` returns a human-readable synopsis of the command-line interface. For example, given:

```csharp
class AppConfig
{
    [ArgsHasParameter("-v|--verbose")]
    public bool Verbose { get; set; }

    [ArgsValueFor("-o|--output")]
    public string? OutputPath { get; set; }

    [ArgsValueFor("--format")]
    public string? Format { get; set; }
}
```

`ArgumentsReader.ToArgsString<AppConfig>()` produces:

```
-v|--verbose [-o|--output <outputpath>] [--format <format>]
```

Required arguments appear unbracketed; optional (nullable) ones are wrapped in `[…]`.

---

## Help generator

`HelpGenerator` produces formatted help text from the `Description` property that can be set on each attribute, for example:

```csharp
[ArgsValueFor("--output", Description = "Path to the output directory")]
public string? OutputPath { get; set; }
```

The text from every attribute's `Description` field is collected and formatted into a help string.

```csharp
ArgumentsReader.OnHelp = async subcmd =>
{
    // subcmd is the subcommand name, or null for the root command
    Console.WriteLine(HelpGenerator.GetHelp<MyConfig>());
};

ArgumentsReader.OnVersion = async () =>
{
    Console.WriteLine("myapp 1.0.0");
};
```

`ArgumentsReader.OnHelp` is an `async` delegate invoked automatically when `--help` (or `-h`) is detected in the arguments. The `subcmd` parameter contains the active subcommand name, or `null` when at the root level.  
`ArgumentsReader.OnVersion` is an `async` delegate invoked automatically when `--version` is detected.

Call `HelpGenerator.GetHelp<T>()` (or the non-generic overload `GetHelp(Type)`) to obtain the help string; the result is cached. Call `HelpGenerator.ClearCache()` to invalidate the cache.

---

## Attributes

| Attribute | Target | Description |
|---|---|---|
| `[ArgsHasParameter("name")]` | `bool` property | `true` when the named flag is present in the arguments |
| `[ArgsValueFor("name")]` | any property | reads the value that follows the named flag |
| `[ArgsValueForBool("true-name", "false-name")]` | `bool` property | sets `true`/`false` depending on which flag is present |
| `[ArgsEnum]` | enum property | maps flag/value arguments to enum members decorated with `[ArgsEnumValue]`; optionally accepts pipe-separated flag names |
| `[ArgsEnumValue("value")]` | enum member | the CLI string value that maps to this enum member |
| `[ArgsAfter("prop1", "prop2", ...)]` | any property | the field can only be assigned a value after **all** of the specified fields have been assigned; once this field receives a value, the specified fields become immutable |
| `[ArgsOneOf("prop1", "prop2", ...)]` | **class** | only one of the listed fields may have a value at a time; all listed fields must be nullable; may be applied multiple times |
| `[ArgsIfSet("prop1", "prop2", ...)]` | any property | the field can only be assigned a value if **all** specified fields are not `null` |
| `[ArgsPathspec]` | `string[]` property | captures all arguments after `--` |
| `[ArgsPositional(index)]` | any property | captures positional arguments by zero-based index |
| `[ArgsObject("name")]` | sub-object property | dispatches to a subcommand object when the named keyword is encountered; supports **classes, records, and structs** with arbitrary nesting depth |
| `[ArgsPipeline]` | any collection of an interface | collects an ordered sequence of pipeline command objects that all implement the property's element interface |
| `[ArgsPipelineCommand("name")]` | class | registers a class as a named pipeline command; the class must implement the interface declared on the `[ArgsPipeline]` property |
| `[ArgsSplit("div1", ...)]` | tuple, `Dictionary<TKey, TValue>`, or collection-of-tuples property | splits a single argument value into parts using the supplied dividers; the first divider separates key from value in dictionaries; for tuples supports a cyclic (default) and a per-part mode via `PartsDividers`; repeated flags populate collections or dictionaries |
| `[ArgsConvertor(typeof(T))]` | any property | applies a custom `IArgsConvertor` to convert the raw string value into the property's type |
| `[ArgsAcceptFromAmong("a", "b", ...)]` | string (or collection of string) property | rejects any value that is not in the supplied set |
| `[ArgsExistingOnlyFile]` | string property | rejects the value if it is not a path to an existing file |
| `[ArgsExistingOnlyDirectory]` | string property | rejects the value if it is not a path to an existing directory |
| `[ArgsLegalFileNamesOnly]` | string property | rejects the value if it contains characters that are illegal in a file name on the current OS |
| *(any `ValidationAttribute`)* | any property | standard `System.ComponentModel.DataAnnotations` attributes (e.g. `[Required]`, `[Range]`, `[EmailAddress]`) are evaluated after parsing |

---

## Environment variable fallback

`[ArgsValueFor]`, `[ArgsHasParameter]`, and `[ArgsEnum]` all accept an optional `EnvVar` property.  
When the named CLI argument is **not present**, the library looks up the environment variable and uses its value as a fallback.  
A command-line argument always takes precedence over an environment variable.

```csharp
class AppConfig
{
    // Falls back to $APP_OUTPUT when --output is absent
    [ArgsValueFor("--output", EnvVar = "APP_OUTPUT")]
    public string? Output { get; set; }

    // Falls back to $APP_VERBOSE; accepts "1"/"true" (case-insensitive) → true, anything else → false, empty → false
    [ArgsHasParameter("--verbose", EnvVar = "APP_VERBOSE")]
    public bool Verbose { get; set; }

    // Falls back to $APP_FORMAT; value must match one of the [ArgsEnumValue] strings on the enum members
    [ArgsEnum("--format", EnvVar = "APP_FORMAT")]
    public OutputFormat? Format { get; set; }
}
```

### `.env` file support

The library automatically reads a `.env` file from the **current working directory** when evaluating environment variable fallbacks.  
Standard `.env` syntax is supported: `KEY=value`, optional surrounding quotes (`"..."` or `'...'`), `#` comment lines, and blank lines.

```
# .env
APP_OUTPUT=/tmp/output
APP_VERBOSE=1
APP_FORMAT=json
```

`.env` values have lower priority than real environment variables: if both define the same key, the real environment variable wins.



### Flags and values

```csharp
// myapp -v --output ./bin --format json
class AppConfig
{
    [ArgsHasParameter("-v|--verbose")]
    public bool Verbose { get; set; }

    [ArgsValueFor("-o|--output")]
    public string? OutputPath { get; set; }

    [ArgsValueFor("--format")]
    public string? Format { get; set; }
}

var (config, errors, position) = ArgumentsReader.ToObject<AppConfig>(args);
// config.Verbose    → true
// config.OutputPath → "./bin"
// config.Format     → "json"
```

### Positional arguments (implicit)

Properties without any attribute receive positional arguments in declaration order.

```csharp
// mv old_path new_path
class MvConfig
{
    public string? OldPath { get; set; }
    public string? NewPath { get; set; }
}
```

### Positional arguments (explicit)

```csharp
// cp src dest
class CpConfig
{
    [ArgsPositional(0)]
    public string? Source { get; set; }

    [ArgsPositional(1)]
    public string? Destination { get; set; }
}
```

### Bool flags with true/false variants

```csharp
// git commit -s  or  git commit --no-signoff
class CommitConfig
{
    [ArgsValueForBool("-s|--signoff", "--no-signoff")]
    public bool SignOff { get; set; }
}
```

### Enum mapping

Decorate each enum member with `[ArgsEnunValue]` and use `[ArgsEnum]` on the property.

```csharp
enum OutputFormat
{
    [ArgsEnumValue("--json")] Json,
    [ArgsEnumValue("--xml")]  Xml,
    [ArgsEnumValue("--csv")]  Csv,
}

class ReportConfig
{
    [ArgsEnum]
    public OutputFormat Format { get; set; }
}

// myapp --xml
// config.Format → OutputFormat.Xml
```

When the enum value is passed as the *argument* to a named flag rather than as a standalone flag, pass the flag name (pipe-separated alternatives) to `[ArgsEnum]`:

```csharp
enum LogLevel { Debug, Info, Warn, Error }

class AppConfig
{
    // myapp --log-level warn
    [ArgsEnum("--log-level|-l", Optional = true, DefaultValue = nameof(LogLevel.Info))]
    public LogLevel LogLevel { get; set; }
}
```

### Pathspec (arguments after `--`)

```csharp
// git diff -- src/ tests/
class DiffConfig
{
    [ArgsPathspec]
    public string[]? Paths { get; set; }
}
// config.Paths → ["src/", "tests/"]
```

### Subcommands (`[ArgsObject]`)

The subcommand keyword is specified directly on `[ArgsObject]`. The sub-object can be a **class, record, or struct**. Nesting depth is not artificially limited (verified working to at least 5 levels).

```csharp
// app connect -u alice -p secret run
class AppConfig
{
    [ArgsObject("connect|-c")]
    public ConnectionConfig Connect { get; set; } = null!;

    [ArgsHasParameter("run")]
    public bool? Run { get; set; }
}

// records and structs work too:
record ConnectionConfig
{
    [ArgsValueFor("-u")]
    public string User { get; set; } = null!;

    [ArgsValueFor("-p")]
    public string Pass { get; set; } = null!;

    // further nesting is supported
    [ArgsObject("tls")]
    public TlsConfig? Tls { get; set; }
}

struct TlsConfig
{
    [ArgsHasParameter("--verify")]
    public bool? Verify { get; set; }
}
```

### Pipeline commands (`[ArgsPipeline]`)

`[ArgsPipeline]` can be applied to any collection property whose element type is an interface. All pipeline command classes must implement that interface and be decorated with `[ArgsPipelineCommand]`.

```csharp
// exec pipeline pull --fetch commit -m "fix" push run
class ExecConfig
{
    [ArgsHasParameter("pipeline")]
    public bool? Pipeline { get; set; }

    [ArgsPipeline]
    public IPipelineCommand[]? Commands { get; set; }

    [ArgsHasParameter("run")]
    public bool? Run { get; set; }
}

interface IPipelineCommand { }

[ArgsPipelineCommand("pull")]
class PullCommand : IPipelineCommand
{
    [ArgsHasParameter("--fetch")]
    public bool? Fetch { get; set; }
}

[ArgsPipelineCommand("commit")]
class CommitCommand : IPipelineCommand
{
    [ArgsValueFor("-m")]
    public string? Message { get; set; }
}

[ArgsPipelineCommand("push")]
class PushCommand : IPipelineCommand
{
    [ArgsHasParameter("--force")]
    public bool? Force { get; set; }
}
```

### Value splitting (`[ArgsSplit]`)

`[ArgsSplit]` works together with `[ArgsValueFor]` on a **value tuple** property. It splits the single string value into the individual tuple elements using the dividers you supply.

#### `PartsDividers = false` (default) — cyclic dividers

All parts are separated using the same dividers, applied cyclically. You only need to supply one divider (or a short pattern) regardless of how many elements the tuple has.

```csharp
// myapp --point "3,7"        => (int, int)        single divider ","
// myapp --rgb "255,128,0"    => (int, int, int)    same divider repeated automatically
// myapp --pair "hello-42"    => (string, int)      single divider "-"
class Config
{
    [ArgsValueFor("--point")]
    [ArgsSplit(",")]
    public (int, int)? Point { get; set; }

    [ArgsValueFor("--rgb")]
    [ArgsSplit(",")]
    public (int, int, int)? Rgb { get; set; }

    [ArgsValueFor("--pair")]
    [ArgsSplit("-")]
    public (string, int)? Pair { get; set; }
}
```

You can also provide a short *repeating pattern* of dividers. For a 4-element tuple with `[ArgsSplit("-", ":")]`, the splits are applied as `-`, `:`, `-` (cycling back to the start).

#### `PartsDividers = true` — per-part dividers

Each divider separates a specific consecutive pair of parts (divider *i* sits between part *i* and part *i+1*). Supply exactly **N − 1** dividers for an N-element tuple; each may be different.

```csharp
// myapp --dsc "1.5_hello.x"   =>  (double, string, char)
// splits:  "1.5" | '_' | "hello" | '.' | "x"
class Config
{
    [ArgsValueFor("--dsc")]
    [ArgsSplit("_", ".", PartsDividers = true)]
    public (double, string, char)? DoubleStringChar { get; set; }
}
```

### Dictionary from repeated flags (`[ArgsSplit]` on `Dictionary<TKey, TValue>`)

When `[ArgsSplit]` is applied to a `Dictionary<TKey, TValue>` property, the **first divider** separates the key from the value within each argument. Any **remaining dividers** apply to the value — they split a tuple value or a collection value exactly like the standalone tuple/collection behaviour. Repeated flags populate the dictionary with multiple entries.

#### Simple value types

```csharp
// myapp --define KEY1=VALUE1 --define KEY2=VALUE2
class AppConfig
{
    [ArgsValueFor("--define")]
    [ArgsSplit("=")]
    public Dictionary<string, string>? Defines { get; set; }
}
// config.Defines → { "KEY1": "VALUE1", "KEY2": "VALUE2" }

// myapp --threshold cpu=90 --threshold memory=75
class AppConfig
{
    [ArgsValueFor("--threshold")]
    [ArgsSplit("=")]
    public Dictionary<string, int>? Thresholds { get; set; }
}
// config.Thresholds → { "cpu": 90, "memory": 75 }
```

#### Tuple value

Dividers after the first one split the tuple elements of the value:

```csharp
// myapp --entry item=hello,42
class AppConfig
{
    [ArgsValueFor("--entry")]
    [ArgsSplit("=", ",")]
    public Dictionary<string, (string, int)>? Entries { get; set; }
}
// config.Entries → { "item": ("hello", 42) }
```

#### Collection value

The second divider also works as a collection element separator:

```csharp
// myapp --tags env=prod,staging,dev --tags region=us,eu
class AppConfig
{
    [ArgsValueFor("--tags")]
    [ArgsSplit("=", ",")]
    public Dictionary<string, string[]>? Tags { get; set; }
}
// config.Tags → { "env": ["prod", "staging", "dev"], "region": ["us", "eu"] }
```

---

### Collection of split values (`[ArgsSplit]` on a collection of tuples)

When the property is a **collection of tuples** (e.g. `(int, int)[]`), each occurrence of the flag is split into one tuple and appended to the collection.

```csharp
// myapp --points "1,2" --points "3,4" --points "5,6"
class AppConfig
{
    [ArgsValueFor("--points")]
    [ArgsSplit(",")]
    public (int, int)[]? Points { get; set; }
}
// config.Points → [(1, 2), (3, 4), (5, 6)]
```

---

### Dictionary from repeated flags (`[ArgsSplit]` on `Dictionary<TKey, TValue>`)

When `[ArgsSplit]` is applied to a `Dictionary<TKey, TValue>` property, the **first divider** separates the key from the value within each argument. Any **remaining dividers** apply to the value — they split a tuple value or a collection value exactly like the standalone tuple/collection behaviour. Repeated flags populate the dictionary with multiple entries.

#### Simple value types

```csharp
// myapp --define KEY1=VALUE1 --define KEY2=VALUE2
class AppConfig
{
    [ArgsValueFor("--define")]
    [ArgsSplit("=")]
    public Dictionary<string, string>? Defines { get; set; }
}
// config.Defines → { "KEY1": "VALUE1", "KEY2": "VALUE2" }

// myapp --threshold cpu=90 --threshold memory=75
class AppConfig
{
    [ArgsValueFor("--threshold")]
    [ArgsSplit("=")]
    public Dictionary<string, int>? Thresholds { get; set; }
}
// config.Thresholds → { "cpu": 90, "memory": 75 }
```

#### Tuple value

Dividers after the first one split the tuple elements of the value:

```csharp
// myapp --entry item=hello,42
class AppConfig
{
    [ArgsValueFor("--entry")]
    [ArgsSplit("=", ",")]
    public Dictionary<string, (string, int)>? Entries { get; set; }
}
// config.Entries → { "item": ("hello", 42) }
```

#### Collection value

The second divider also works as a collection element separator:

```csharp
// myapp --tags env=prod,staging,dev --tags region=us,eu
class AppConfig
{
    [ArgsValueFor("--tags")]
    [ArgsSplit("=", ",")]
    public Dictionary<string, string[]>? Tags { get; set; }
}
// config.Tags → { "env": ["prod", "staging", "dev"], "region": ["us", "eu"] }
```

---

### Collection of split values (`[ArgsSplit]` on a collection of tuples)

When the property is a **collection of tuples** (e.g. `(int, int)[]`), each occurrence of the flag is split into one tuple and appended to the collection.

```csharp
// myapp --points "1,2" --points "3,4" --points "5,6"
class AppConfig
{
    [ArgsValueFor("--points")]
    [ArgsSplit(",")]
    public (int, int)[]? Points { get; set; }
}
// config.Points → [(1, 2), (3, 4), (5, 6)]
```

---

### Repeated flags as a collection

When the same flag appears multiple times, you can collect all its values into an array or list by declaring the property as a collection type.

```csharp
// myapp --exclude foo --exclude bar --exclude baz
class AppConfig
{
    [ArgsValueFor("--exclude")]
    public string[]? Exclude { get; set; }
}
// config.Exclude → ["foo", "bar", "baz"]
```

Any type that implements `ICollection<T>` is supported (e.g. `List<string>`, `string[]`).

---

### Common types

Out-of-the-box value conversion for rich .NET types:

```csharp
// exec --only-date 2024-06-15 --time 14:30:00 --span 01:30:00
//      --file notes.txt --dir /tmp --url https://example.com
//      --guid d3b07384-d9a5-4e7d-a8d0-1234567890ab
//      --ip 192.168.1.1 --version 1.2.3.4
class CommonTypesConfig
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

    [ArgsValueFor("--dir")]
    public DirectoryInfo? Dir { get; set; }

    [ArgsValueFor("--url")]
    public Uri? Url { get; set; }

    [ArgsValueFor("--guid")]
    public Guid? Guid { get; set; }

    [ArgsValueFor("--ip")]
    public System.Net.IPAddress? Ip { get; set; }

    [ArgsValueFor("--version")]
    public Version? Version { get; set; }
}
```

### Mutual exclusion and conditional requirements

`[ArgsOneOf]` is a **class-level** attribute. It takes a list of field names; only one of the listed fields may have a value at a time. All listed fields must be nullable. It can be applied multiple times to the same class for independent groups.  
`[ArgsAfter]` requires that all listed fields are assigned before the decorated field can receive its value. Once the decorated field is assigned, the listed fields become immutable.  
`[ArgsIfSet]` allows the decorated field to be assigned only when all specified fields are not `null`.

```csharp
[ArgsOneOf(nameof(File), nameof(Message))]   // exactly one of File/Message may be set
class CommitConfig
{
    [ArgsValueFor("-F|--file", Optional = true)]
    public string? File { get; set; }

    [ArgsValueFor("-m|--message", Optional = true)]
    public string? Message { get; set; }

    [ArgsHasParameter("--pathspec-file-nul")]
    [ArgsIfSet(nameof(File))]          // can only be set if File is not null
    public bool PathspecFileNul { get; set; }
}
```

### Custom convertor (`[ArgsConvertor]`)

When the built-in type conversion is not sufficient, implement `IArgsConvertor` and reference it with `[ArgsConvertor]`.

```csharp
// myapp --ip 192.168.1.1
public class IPv4ToIntArrayConvertor : IArgsConvertor
{
    public object Convert(string value)
    {
        // <some magic appears here>
        return result;
    }
}

class AppConfig
{
    [ArgsValueFor("--ip")]
    [ArgsConvertor(typeof(IPv4ToIntArrayConvertor))]
    public int[]? IpAddress { get; set; }
}
// config.IpAddress → [192, 168, 1, 1]
```

---

### Value validation attributes

#### `[ArgsAcceptFromAmong]`

Restricts the accepted values to a fixed set. When the parsed value is not in the list, an error is returned in the `errors` array (no exception is thrown). Works with both scalar and collection properties.

```csharp
// myapp --format png
class AppConfig
{
    [ArgsValueFor("--format")]
    [ArgsAcceptFromAmong("jpg", "png", "gif")]
    public string? FileExtension { get; set; }

    // myapp --formats jpg --formats gif
    [ArgsValueFor("--formats")]
    [ArgsAcceptFromAmong("jpg", "png", "gif")]
    public string[]? FileExtensions { get; set; }
}
```

#### `[ArgsExistingOnlyFile]`

Rejects the value unless it is a path to an **existing file** on the filesystem.

```csharp
class AppConfig
{
    [ArgsValueFor("--file")]
    [ArgsExistingOnlyFile]
    public string? FilePath { get; set; }
}
```

#### `[ArgsExistingOnlyDirectory]`

Rejects the value unless it is a path to an **existing directory** on the filesystem.

```csharp
class AppConfig
{
    [ArgsValueFor("--dir")]
    [ArgsExistingOnlyDirectory]
    public string? DirPath { get; set; }
}
```

#### `[ArgsLegalFileNamesOnly]`

Rejects the value if it contains characters that are **illegal in a file name** on the current operating system.

```csharp
class AppConfig
{
    [ArgsValueFor("--name")]
    [ArgsLegalFileNamesOnly]
    public string? FileName { get; set; }
}
```

---

### `System.ComponentModel.DataAnnotations` support

Any standard `ValidationAttribute` (e.g. `[Required]`, `[Range]`, `[EmailAddress]`, `[MaxLength]`, `[RegularExpression]`) placed on a property is evaluated after parsing. Validation failures are returned as errors in the `errors` array of the result tuple.

```csharp
using System.ComponentModel.DataAnnotations;

class UserConfig
{
    [ArgsValueFor("--name")]
    [Required]
    public string Name { get; set; } = "";

    [ArgsValueFor("--email")]
    [EmailAddress]
    public string? Email { get; set; }

    [ArgsValueFor("--count")]
    [Range(1, 100)]
    public int Count { get; set; }

    [ArgsValueFor("--tag")]
    [MaxLength(10)]
    public string? Tag { get; set; }

    [ArgsValueFor("--code")]
    [RegularExpression("^[A-Z]{2,5}$")]
    public string? Code { get; set; }
}
```

---

### Inline and combined short flags

The library understands both common short-flag styles:

```
-o./bin        →  -o ./bin
-am            →  -a -m
--output=./bin →  --output ./bin
```

---

## License

[MIT](LICENSE) © 2026 Mansiper

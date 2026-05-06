# ArgsToConfig

**ArgsToConfig** is a .NET library that maps CLI arguments directly onto a strongly-typed configuration class using attributes.  
No manual parsing loops — just decorate your properties and call `ArgumentsReader.ToObject<T>()`.

> 📦 NuGet: *link coming soon*

---

## Installation

```
dotnet add package ArgsToConfig
```

---

## Quick start

```csharp
var config = ArgumentsReader.ToObject<MyConfig>(args);
```

Decorate the properties of `MyConfig` with the appropriate attributes (see below) and the library handles the rest.

---

## Attributes

| Attribute | Target | Description |
|---|---|---|
| `[ArgsHasParameter("name")]` | `bool` property | `true` when the named flag is present in the arguments |
| `[ArgsValueFor("name")]` | any property | reads the value that follows the named flag |
| `[ArgsValueForBool("true-name", "false-name")]` | `bool` property | sets `true`/`false` depending on which flag is present |
| `[ArgsEnum]` | enum property | maps flag/value arguments to enum members decorated with `[ArgsValue]`; optionally accepts pipe-separated flag names and an `optional` parameter |
| `[ArgsValue("value")]` | enum member | the CLI string value that maps to this enum member |
| `[ArgsAfter("prop1", "prop2", ...)]` | any property | captures the first positional argument that appears after **any** of the listed fields are set; trying to set any of those fields afterwards throws an exception |
| `[ArgsOneOf("prop1", "prop2", ...)]` | any property | declares that exactly one of the listed fields plus this field must be set; apply it to the **last** field in the group |
| `[ArgsIfSet("prop")]` | any property | requires another property to be set when this one is set |
| `[ArgsPathspec]` | `string[]` property | captures all arguments after `--` |
| `[ArgsPositional(index)]` | any property | captures positional arguments by zero-based index |
| `[ArgsObject("name")]` | sub-object property | dispatches to a subcommand class when the named keyword is encountered; the subcommand name is defined here, not on the class |
| `[ArgsPipeline]` | any collection of an interface | collects an ordered sequence of pipeline command objects that all implement the property's element interface |
| `[ArgsPipelineCommand("name")]` | class | registers a class as a named pipeline command; the class must implement the interface declared on the `[ArgsPipeline]` property |

---

## Examples

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

var config = ArgumentsReader.ToObject<AppConfig>(args);
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

Decorate each enum member with `[ArgsValue]` and use `[ArgsEnum]` on the property.

```csharp
enum OutputFormat
{
    [ArgsValue("--json")] Json,
    [ArgsValue("--xml")]  Xml,
    [ArgsValue("--csv")]  Csv,
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
    [ArgsEnum("--log-level|-l", optional: true, DefaultValue = nameof(LogLevel.Info))]
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

The subcommand keyword is specified directly on `[ArgsObject]`. `ArgsObjectRoot` is no longer used.

```csharp
// app connect -u alice -p secret run
class AppConfig
{
    [ArgsObject("connect")]
    public ConnectionConfig Connect { get; set; } = null!;

    [ArgsHasParameter("run")]
    public bool? Run { get; set; }
}

class ConnectionConfig
{
    [ArgsValueFor("-u")]
    public string User { get; set; } = null!;

    [ArgsValueFor("-p")]
    public string Pass { get; set; } = null!;
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

### Mutual exclusion and conditional requirements

`[ArgsOneOf]` takes a string array of field names and must be placed on the **last** field in the group.  
`[ArgsAfter]` captures the first positional argument that appears after **any** of the listed fields are set; attempting to set any of those fields again after that throws an exception.

```csharp
class CommitConfig
{
    [ArgsValueFor("-F|--file", optional: true)]
    public string? File { get; set; }

    [ArgsValueFor("-m|--message", optional: true)]
    [ArgsOneOf(nameof(File))]          // exactly one of File/Message must be set; apply to the last field
    public string? Message { get; set; }

    [ArgsHasParameter("--pathspec-file-nul")]
    [ArgsIfSet(nameof(File))]          // error if set without File
    public bool PathspecFileNul { get; set; }
}
```

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

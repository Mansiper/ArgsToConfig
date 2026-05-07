# CLI arguments to configuration

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

> 💡 **Optional arguments** — if an argument is optional, simply declare the property as a nullable type (e.g. `string?`, `bool?`, `int?`). When the argument is absent, the property will remain `null`.

---

## Attributes

| Attribute | Target | Description |
|---|---|---|
| `[ArgsHasParameter("name")]` | `bool` property | `true` when the named flag is present in the arguments |
| `[ArgsValueFor("name")]` | any property | reads the value that follows the named flag |
| `[ArgsValueForBool("true-name", "false-name")]` | `bool` property | sets `true`/`false` depending on which flag is present |
| `[ArgsEnum]` | enum property | maps flag/value arguments to enum members decorated with `[ArgsValue]`; optionally accepts pipe-separated flag names and an `optional` parameter |
| `[ArgsValue("value")]` | enum member | the CLI string value that maps to this enum member |
| `[ArgsAfter("prop1", "prop2", ...)]` | any property | the field can only be assigned a value after **all** of the specified fields have been assigned; once this field receives a value, the specified fields become immutable — attempting to change them throws an exception |
| `[ArgsOneOf("prop1", "prop2", ...)]` | nullable property | only one of the specified fields and the field it is applied to may have a value at a time; apply it to the **last** field in the group; all fields in the group must be nullable |
| `[ArgsIfSet("prop1", "prop2", ...)]` | any property | the field can only be assigned a value if **all** specified fields are not `null` |
| `[ArgsPathspec]` | `string[]` property | captures all arguments after `--` |
| `[ArgsPositional(index)]` | any property | captures positional arguments by zero-based index |
| `[ArgsObject("name")]` | sub-object property | dispatches to a subcommand class when the named keyword is encountered; the subcommand name is defined here, not on the class |
| `[ArgsPipeline]` | any collection of an interface | collects an ordered sequence of pipeline command objects that all implement the property's element interface |
| `[ArgsPipelineCommand("name")]` | class | registers a class as a named pipeline command; the class must implement the interface declared on the `[ArgsPipeline]` property |
| `[ArgsTuple("div1", ...)]` | tuple property | splits a single argument value into the elements of a value tuple using the supplied dividers; supports a cyclic (default) and a per-part mode via `PartsDividers` |
| `[ArgsConvertor(typeof(T))]` | any property | applies a custom `IArgsConvertor` to convert the raw string value into the property's type |
| `[ArgsAcceptFromAmong("a", "b", ...)]` | string (or collection of string) property | rejects any value that is not in the supplied set; throws `ArgumentException` otherwise |
| `[ArgsExistingOnlyFile]` | string property | rejects the value if it is not a path to an existing file |
| `[ArgsExistingOnlyDirectory]` | string property | rejects the value if it is not a path to an existing directory |
| `[ArgsLegalFileNamesOnly]` | string property | rejects the value if it contains characters that are illegal in a file name on the current OS |
| *(any `ValidationAttribute`)* | any property | standard `System.ComponentModel.DataAnnotations` attributes (e.g. `[Required]`, `[Range]`, `[EmailAddress]`) are evaluated after parsing and throw `ValidationException` on failure |

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

### Tuple splitting (`[ArgsTuple]`)

`[ArgsTuple]` works together with `[ArgsValueFor]` on a **value tuple** property. It splits the single string value into the individual tuple elements using the dividers you supply.

#### `PartsDividers = false` (default) — cyclic dividers

All parts are separated using the same dividers, applied cyclically. You only need to supply one divider (or a short pattern) regardless of how many elements the tuple has.

```csharp
// myapp --point "3,7"        => (int, int)        single divider ","
// myapp --rgb "255,128,0"    => (int, int, int)    same divider repeated automatically
// myapp --pair "hello-42"    => (string, int)      single divider "-"
class Config
{
    [ArgsValueFor("--point")]
    [ArgsTuple(",")]
    public (int, int)? Point { get; set; }

    [ArgsValueFor("--rgb")]
    [ArgsTuple(",")]
    public (int, int, int)? Rgb { get; set; }

    [ArgsValueFor("--pair")]
    [ArgsTuple("-")]
    public (string, int)? Pair { get; set; }
}
```

You can also provide a short *repeating pattern* of dividers. For a 4-element tuple with `[ArgsTuple("-", ":")]`, the splits are applied as `-`, `:`, `-` (cycling back to the start).

#### `PartsDividers = true` — per-part dividers

Each divider separates a specific consecutive pair of parts (divider *i* sits between part *i* and part *i+1*). Supply exactly **N − 1** dividers for an N-element tuple; each may be different.

```csharp
// myapp --dsc "1.5_hello.x"   =>  (double, string, char)
// splits:  "1.5" | '_' | "hello" | '.' | "x"
class Config
{
    [ArgsValueFor("--dsc")]
    [ArgsTuple("_", ".", PartsDividers = true)]
    public (double, string, char)? DoubleStringChar { get; set; }
}
```

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

### Mutual exclusion and conditional requirements

`[ArgsOneOf]` takes a list of field names and must be placed on the **last** field in the group. Only one of the listed fields (including the decorated one) may have a value; all must be nullable.  
`[ArgsAfter]` requires that all listed fields are assigned before the decorated field can receive its value. Once the decorated field is assigned, the listed fields become immutable — any attempt to change them throws an exception.  
`[ArgsIfSet]` allows the decorated field to be assigned only when all specified fields are not `null`.

```csharp
class CommitConfig
{
    [ArgsValueFor("-F|--file", optional: true)]
    public string? File { get; set; }

    [ArgsValueFor("-m|--message", optional: true)]
    [ArgsOneOf(nameof(File))]          // exactly one of File/Message must be set; apply to the last field
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

Restricts the accepted values to a fixed set. Throws `ArgumentException` when the parsed value is not in the list. Works with both scalar and collection properties.

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

Any standard `ValidationAttribute` (e.g. `[Required]`, `[Range]`, `[EmailAddress]`, `[MaxLength]`, `[RegularExpression]`) placed on a property is evaluated after parsing. A `ValidationException` is thrown when a constraint is violated.

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

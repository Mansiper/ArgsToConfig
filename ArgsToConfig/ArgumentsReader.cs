using System.ComponentModel.DataAnnotations;

namespace ArgsToConfig;

public static class ArgumentsReader
{
    public static (T? result, string[]? errors, int? position) ToObject<T>(params string[] args) where T : new()
    {
        CheckHelpVersion(args);

        var obj = new T();
        var rules = InnerToObject.BuildRules(typeof(T));
        var (error, position) = InnerToObject.ApplyRules(obj, rules, args);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(obj, new ValidationContext(obj), validationResults, validateAllProperties: true))
            return (default, validationResults.Select(r => r.ErrorMessage ?? "Validation error").ToArray(), null);
        return (obj, error is null ? null : [error], position + 1);
    }

    public static string[] ToArgs<T>(T obj) where T : new()
    {
        if (obj is null) throw new ArgumentNullException(nameof(obj));

        var result = new List<string>();
        InnerToArgs.BuildArgs(obj, result);
        return result.ToArray();
    }

    /// <summary>
    /// Returns a synopsis string describing the command-line interface of <typeparamref name="T"/>,
    /// in the style of <c>git commit [-a | --interactive] [-s] [-v] [--flag &lt;value&gt;]</c>.
    /// </summary>
    public static string ToArgsString<T>() where T : new()
    {
        var tokens = new List<string>();
        InnerToArgs.BuildArgsString(typeof(T), tokens);
        return string.Join(" ", tokens);
    }

    /// <summary>
    /// Optional callback for handling help requests.
    /// Receives the subcommand name if specified (e.g. "myapp --help subcmd"), or null if no subcommand was specified (e.g. "myapp --help").
    /// </summary>
    public static Func<string?, Task>? OnHelp { get; set; }

    /// <summary>
    /// Optional callback for handling version requests.
    /// </summary>
    public static Func<Task>? OnVersion { get; set; }

    private static void CheckHelpVersion(string[] args)
    {
        if (args.Length is 1 or 2 && (args[0] == "--help" || args[0] == "-h") && OnHelp is not null)
        {
            OnHelp(args.Length == 1 ? null : args[1]).Wait();
            Environment.Exit(0);
        }
        if (args.Length == 1 && (args[0] == "--version" || args[0] == "-v") && OnVersion is not null)
        {
            OnVersion().Wait();
            Environment.Exit(0);
        }
    }
}
// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace ArgsToConfig;

public static class ArgumentsReader
{
    /// <summary>
    /// Parses the given command-line arguments into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the arguments into. Must have a parameterless constructor.</typeparam>
    /// <param name="args">The command-line arguments to parse.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description><c>result</c> — the populated object, or <see langword="default"/> if validation failed.</description></item>
    /// <item><description><c>errors</c> — an array of error messages, or <see langword="null"/> if parsing succeeded.</description></item>
    /// <item><description><c>position</c> — the index of the next unprocessed argument, or <see langword="null"/> if validation failed.</description></item>
    /// </list>
    /// </returns>
    public static (T? result, string[]? errors, int? position) ToObject<T>(params string[] args) where T : new()
    {
        CheckHelpVersion(args);

        var obj = new T();
        var rules = InnerToObject.BuildRules(typeof(T));
        var (error, position) = InnerToObject.ApplyRules(obj, rules, args, onUnknownArgument: OnUnknownArgument);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(obj, new ValidationContext(obj), validationResults, validateAllProperties: true))
            return (default, validationResults.Select(r => r.ErrorMessage ?? "Validation error").ToArray(), null);
        return (obj, error is null ? null : [error], position + 1);
    }

    /// <summary>
    /// Serializes an instance of <typeparamref name="T"/> into an array of command-line argument strings.
    /// </summary>
    /// <typeparam name="T">The type to serialize. Must have a parameterless constructor.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>An array of command-line argument strings representing the object's state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is <see langword="null"/>.</exception>
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
    /// Receives the subcommand name if specified (e.g. <c>myapp --help subcmd</c>), or <see langword="null"/> if no subcommand was specified (e.g. <c>myapp --help</c>).
    /// When set and a help flag is detected, the callback is invoked and the process exits.
    /// </summary>
    public static Func<string?, Task>? OnHelp { get; set; }

    /// <summary>
    /// Optional callback for handling version requests.
    /// When set and a version flag (<c>--version</c> or <c>-v</c>) is detected, the callback is invoked and the process exits.
    /// </summary>
    public static Func<Task>? OnVersion { get; set; }
    
    /// <summary>
    /// Optional callback for handling unknown arguments.
    /// Receives the unknown argument string. Return <see langword="true"/> to continue parsing the remaining arguments,
    /// or <see langword="false"/> to stop and exit the process.
    /// When not set, an error is returned normally in the <c>errors</c> array.
    /// </summary>
    public static Func<string, Task<bool>>? OnUnknownArgument { get; set; }

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
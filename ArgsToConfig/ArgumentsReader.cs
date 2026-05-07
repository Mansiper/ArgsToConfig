using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ArgsToConfig.Attributes;
using ArgsToConfig.Models;

namespace ArgsToConfig;

public static class ArgumentsReader
{
    public static T ToObject<T>(params string[] args) where T : new()
    {
        CheckHelpVersion(args);

        var obj = new T();
        var rules = BuildRules(typeof(T));
        ApplyRules(obj, rules, args);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(obj, new ValidationContext(obj), validationResults, validateAllProperties: true))
            throw new ValidationException(string.Join(Environment.NewLine, validationResults.Select(r => r.ErrorMessage)));
        return obj;
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

    private static List<PropertyRule> BuildRules(Type type)
    {
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var rules = new List<PropertyRule>(props.Length);

        foreach (var prop in props)
        {
            var hasParam = prop.GetCustomAttribute<ArgsHasParameterAttribute>();
            var valueFor = prop.GetCustomAttribute<ArgsValueForAttribute>();
            var valueForBool = prop.GetCustomAttribute<ArgsValueForBoolAttribute>();
            var argsEnum = prop.GetCustomAttribute<ArgsEnumAttribute>();
            var argsTuple = prop.GetCustomAttribute<ArgsTupleAttribute>();
            var isEnum = argsEnum is not null;
            var after = prop.GetCustomAttribute<ArgsAfterAttribute>();
            var oneOf = prop.GetCustomAttribute<ArgsOneOfAttribute>();
            var ifSet = prop.GetCustomAttribute<ArgsIfSetAttribute>();
            var isPathspec = prop.GetCustomAttribute<ArgsPathspecAttribute>() is not null;
            var isObject = prop.GetCustomAttribute<ArgsObjectAttribute>();
            var positional = prop.GetCustomAttribute<ArgsPositionalAttribute>();
            var convertor = prop.GetCustomAttribute<ArgsConvertorAttribute>();
            var isExistingOnlyFile = prop.GetCustomAttribute<ArgsExistingOnlyFileAttribute>() is not null;
            var isExistingOnlyDirectory = prop.GetCustomAttribute<ArgsExistingOnlyDirectoryAttribute>() is not null;
            var isLegalFileNamesOnly = prop.GetCustomAttribute<ArgsLegalFileNamesOnlyAttribute>() is not null;
            var acceptFromAmong = prop.GetCustomAttribute<ArgsAcceptFromAmongAttribute>();

            // ArgsObject – sub-object with root name on the attribute
            if (isObject is not null)
            {
                rules.Add(new PropertyRule
                {
                    Property = prop,
                    IsObject = true,
                    ObjectRootName = isObject.GetName
                });
                continue;
            }

            // ArgsPipeline – array of interface instances
            var isPipeline = prop.GetCustomAttribute<ArgsPipelineAttribute>() is not null;
            if (isPipeline)
            {
                var arrayType = prop.PropertyType;
                Type elementType;
                if (arrayType.IsArray)
                    elementType = arrayType.GetElementType()!;
                else if (arrayType.IsGenericType)
                    elementType = arrayType.GetGenericArguments()[0];
                else
                    elementType = arrayType;
                rules.Add(new PropertyRule
                {
                    Property = prop,
                    IsPipeline = true,
                    PipelineElementType = elementType
                });
                continue;
            }

            // Enum type – build member rules
            EnumMemberRule[]? enumMemberRules = null;
            if (isEnum)
            {
                var enumType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (enumType.IsEnum)
                {
                    var members = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
                    enumMemberRules = members.Select(m =>
                    {
                        var mHas = m.GetCustomAttribute<ArgsHasParameterAttribute>();
                        var mVal = m.GetCustomAttribute<ArgsValueAttribute>();
                        return new EnumMemberRule
                        {
                            Value = m.GetValue(null)!,
                            HasParameterNames = mHas?.GetNames,
                            ArgsValue = mVal?.GetValue
                        };
                    }).ToArray();

                    // Promote ArgsEnum name params to valueFor if specified on the attribute
                    if (argsEnum!.GetNames is not null && valueFor is null)
                        valueFor = new ArgsValueForAttribute(string.Join("|", argsEnum.GetNames), argsEnum.GetOptional)
                        {
                            DefaultValue = argsEnum.DefaultValue
                        };
                }
            }

            rules.Add(new PropertyRule
            {
                Property = prop,
                HasParameterNames = hasParam?.GetNames,
                HasParameterPosition = hasParam?.GetPosition ?? -1,
                ValueForNames = valueFor?.GetNames,
                ValueForOptional = valueFor?.GetOptional ?? false,
                ValueForDefault = valueFor?.DefaultValue,
                ValueForBoolTrueNames = valueForBool?.GetTrueNames,
                ValueForBoolFalseNames = valueForBool?.GetFalseNames,
                IsEnum = isEnum,
                AfterFields = after?.GetFields,
                OneOfFields = oneOf?.GetFields,
                IfSetFields = ifSet?.GetFields,
                IsPathspec = isPathspec,
                EnumMemberRules = enumMemberRules,
                PositionalIndex = positional?.GetPosition ?? -1,
                TupleDividers = argsTuple?.GetDividers,
                TuplePartsDividers = argsTuple?.PartsDividers ?? false,
                IsImplicitPositional = hasParam is null && valueFor is null && valueForBool is null
                    && !isEnum && after is null && !isPathspec && positional is null,
                ConvertorType = convertor?.GetConvertorType,
                IsExistingOnlyFile = isExistingOnlyFile,
                IsExistingOnlyDirectory = isExistingOnlyDirectory,
                IsLegalFileNamesOnly = isLegalFileNamesOnly,
                AcceptFromAmong = acceptFromAmong?.GetValues
            });
        }

        return rules;
    }

    private static void ApplyRules(object obj, List<PropertyRule> rules, string[] args, bool ignoreUnknown = false)
    {
        // ── ArgsObject subcommand dispatch ───────────────────────────────────
        var objectRules = rules.Where(r => r.IsObject).ToList();
        if (objectRules.Count > 0)
        {
            // Build a map: subcommand name → rule
            var subcmdMap = new Dictionary<string, PropertyRule>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in objectRules)
                if (r.ObjectRootName is not null)
                    subcmdMap[r.ObjectRootName] = r;

            var nonObjectRules = rules.Where(r => !r.IsObject).ToList();
            var parentArgNames = BuildKnownArgNames(nonObjectRules);

            // Collect root positional (non-dash) parameter names so we can reclaim them from subcommand segments
            var rootPositionalNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in nonObjectRules)
                if (r.HasParameterNames is not null)
                    foreach (var n in r.HasParameterNames.Where(n => !n.StartsWith('-')))
                        rootPositionalNames.Add(n);

            // Validate: no arg name conflicts between parent non-object rules and any subcommand
            var subRulesCache = new Dictionary<Type, List<PropertyRule>>();
            foreach (var (subcmdName2, subRule) in subcmdMap)
            {
                var subType2 = Nullable.GetUnderlyingType(subRule.Property.PropertyType) ?? subRule.Property.PropertyType;
                if (!subRulesCache.ContainsKey(subType2))
                    subRulesCache[subType2] = BuildRules(subType2);
                var subArgNames2 = BuildKnownArgNames(subRulesCache[subType2]);
                var conflicts2 = parentArgNames.Intersect(subArgNames2, StringComparer.OrdinalIgnoreCase).ToList();
                if (conflicts2.Count > 0)
                    throw new ArgumentException(
                        $"Argument name conflict between root and subcommand '{subcmdName2}': {string.Join(", ", conflicts2)}.");
            }

            // Segment args by subcommand keywords; non-subcommand args go to the root
            // Each segment: (rule, startIndex, args slice)
            var segments = new List<(PropertyRule Rule, List<string> SegArgs)>();
            List<string>? currentSegArgs = null;
            var rootArgs = new List<string>();

            foreach (var a in args)
            {
                if (!a.StartsWith('-') && subcmdMap.TryGetValue(a, out var foundRule))
                {
                    // Start a new segment for this subcommand
                    var currentRule = foundRule;
                    currentSegArgs = [];
                    segments.Add((currentRule, currentSegArgs));
                }
                else if (currentSegArgs is not null && !a.StartsWith('-') && rootPositionalNames.Contains(a))
                    // Root positional name encountered inside a subcommand segment — belongs to root
                    rootArgs.Add(a);
                else if (currentSegArgs is not null)
                    currentSegArgs.Add(a);
                else
                    rootArgs.Add(a);
            }

            if (segments.Count == 0)
                throw new ArgumentException("No subcommand specified.");

            // Apply each subcommand segment
            foreach (var (segRule, segArgs) in segments)
            {
                var subType = Nullable.GetUnderlyingType(segRule.Property.PropertyType) ?? segRule.Property.PropertyType;
                var subObj = Activator.CreateInstance(subType)!;
                if (!subRulesCache.ContainsKey(subType))
                    subRulesCache[subType] = BuildRules(subType);
                var subRules = subRulesCache[subType];
                ApplyRules(subObj, subRules, segArgs.ToArray());
                segRule.Property.SetValue(obj, subObj);
            }

            // Process non-object rules against root args (args before any subcommand)
            if (nonObjectRules.Count > 0)
                ApplyRules(obj, nonObjectRules, rootArgs.ToArray(), ignoreUnknown: true);
            return;
        }

        // ── ArgsPipeline dispatch ────────────────────────────────────────────
        var pipelineRules = rules.Where(r => r.IsPipeline).ToList();
        if (pipelineRules.Count > 0)
        {
            // Build a map from pipeline interface → pipeline rule (one per interface type)
            // and validate: only one pipeline property per interface type
            var pipelineByInterface = new Dictionary<Type, PropertyRule>();
            foreach (var pr in pipelineRules)
            {
                var iface = pr.PipelineElementType!;
                if (!pipelineByInterface.TryAdd(iface, pr))
                    throw new ArgumentException(
                        $"Multiple [ArgsPipeline] properties use the same interface '{iface.Name}'. Collections in the root level must be based on different interfaces.");
            }

            // Collect all [ArgsPipelineCommand] types from the assembly that implement any pipeline interface
            var objAssembly = obj.GetType().Assembly;
            var readerAssembly = typeof(ArgumentsReader).Assembly;
            var callingAssemblies = objAssembly == readerAssembly
                ? new[] { objAssembly }
                : new[] { objAssembly, readerAssembly };
            var allCommandTypes = callingAssemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttribute<ArgsPipelineCommandAttribute>() is not null)
                .ToList();

            // Build maps: commandName → (commandType, pipelineRule)
            // Validate: duplicate command name across same interface → error
            var commandByName = new Dictionary<string, (Type Type, PropertyRule PipelineRule)>(StringComparer.OrdinalIgnoreCase);
            foreach (var cmdType in allCommandTypes)
            {
                var cmdAttr = cmdType.GetCustomAttribute<ArgsPipelineCommandAttribute>()!;
                var cmdName = cmdAttr.GetName;
                // Find which pipeline interface this type implements
                PropertyRule? matchingPipelineRule = null;
                foreach (var (iface, pr) in pipelineByInterface)
                    if (iface.IsAssignableFrom(cmdType))
                    {
                        matchingPipelineRule = pr;
                        break;
                    }
                if (matchingPipelineRule is null)
                    continue; // belongs to a different pipeline in a different class, skip

                if (commandByName.TryGetValue(cmdName, out _))
                    throw new ArgumentException(
                        $"Duplicate [ArgsPipelineCommand] name '{cmdName}' found across pipeline properties.");
                commandByName[cmdName] = (cmdType, matchingPipelineRule);
            }

            // Collect known root arg names (from non-pipeline rules)
            var nonPipelineRules = rules.Where(r => !r.IsPipeline).ToList();
            // Also collect root positional parameter names (like "run", "pipeline")
            var rootPositionalNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in nonPipelineRules)
                if (r.HasParameterNames is not null)
                    foreach (var n in r.HasParameterNames.Where(n => !n.StartsWith('-')))
                        rootPositionalNames.Add(n);

            // Validate: pipeline command names must not collide with root positional parameter names
            foreach (var cmdName in commandByName.Keys.Where(rootPositionalNames.Contains))
                throw new ArgumentException($"Pipeline command name '{cmdName}' conflicts with a root parameter name.");

            // Validate: no pipeline command should use another pipeline command name as an argument name
            var cmdRulesCache = new Dictionary<Type, List<PropertyRule>>();
            foreach (var (cmdName, (cmdType, _)) in commandByName)
            {
                if (!cmdRulesCache.ContainsKey(cmdType))
                    cmdRulesCache[cmdType] = BuildRules(cmdType);
                var cmdRulesCheck = cmdRulesCache[cmdType];
                // Also check positional HasParameter names
                var cmdPositionalNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in cmdRulesCheck)
                    if (r.HasParameterNames is not null)
                        foreach (var n in r.HasParameterNames.Where(n => !n.StartsWith('-')))
                            cmdPositionalNames.Add(n);
                foreach (var otherCmdName in commandByName.Keys)
                    if (!string.Equals(otherCmdName, cmdName, StringComparison.OrdinalIgnoreCase) && cmdPositionalNames.Contains(otherCmdName))
                        throw new ArgumentException($"Pipeline command '{cmdName}' uses another pipeline command name '{otherCmdName}' as an argument.");
            }

            // Segment args: each segment starts when a pipeline command name is encountered
            // Non-pipeline args are collected separately for the root
            var pipelineCommands = new Dictionary<PropertyRule, List<object>>();
            foreach (var pr in pipelineRules)
                pipelineCommands[pr] = [];

            // Track closed pipelines for ordering constraint:
            // once you switch away from a pipeline, it is closed and cannot be re-entered
            PropertyRule? currentPipelineRule = null;
            var closedPipelineRules = new HashSet<PropertyRule>();

            var remainingArgs = new List<string>();
            var i2 = 0;
            while (i2 < args.Length)
            {
                var a = args[i2];
                if (!a.StartsWith('-') && commandByName.TryGetValue(a, out var cmdEntry))
                {
                    if (currentPipelineRule is not null && currentPipelineRule != cmdEntry.PipelineRule)
                        // Switching to a different pipeline: close the current one
                        closedPipelineRules.Add(currentPipelineRule);
                    if (closedPipelineRules.Contains(cmdEntry.PipelineRule))
                        throw new ArgumentException(
                            $"Cannot go back to a previously used pipeline. Command '{a}' belongs to a pipeline that was already closed.");
                    currentPipelineRule = cmdEntry.PipelineRule;

                    // Collect args for this command until next command name or root positional or end
                    i2++;
                    var cmdArgsList = new List<string>();
                    while (i2 < args.Length)
                    {
                        var next = args[i2];
                        if (!next.StartsWith('-') && (commandByName.ContainsKey(next) || rootPositionalNames.Contains(next)))
                            break;
                        cmdArgsList.Add(next);
                        i2++;
                    }
                    var cmdObj = Activator.CreateInstance(cmdEntry.Type)!;
                    if (!cmdRulesCache.ContainsKey(cmdEntry.Type))
                        cmdRulesCache[cmdEntry.Type] = BuildRules(cmdEntry.Type);
                    var cmdRules = cmdRulesCache[cmdEntry.Type];
                    ApplyRules(cmdObj, cmdRules, cmdArgsList.ToArray());
                    pipelineCommands[cmdEntry.PipelineRule].Add(cmdObj);
                }
                else
                {
                    remainingArgs.Add(a);
                    i2++;
                }
            }

            // Set pipeline array/list properties
            foreach (var (pr, list) in pipelineCommands)
            {
                if (list.Count > 0)
                {
                    var elementType = pr.PipelineElementType!;
                    var propType = pr.Property.PropertyType;
                    if (propType.IsArray)
                    {
                        var array = Array.CreateInstance(elementType, list.Count);
                        for (var ai = 0; ai < list.Count; ai++)
                            array.SetValue(list[ai], ai);
                        pr.Property.SetValue(obj, array);
                    }
                    else if (propType.IsGenericType)
                    {
                        var genericDef = propType.GetGenericTypeDefinition();

                        // Interface collection types: materialise as List<T>
                        var listType = typeof(List<>).MakeGenericType(elementType);
                        if (genericDef == typeof(IEnumerable<>)
                            || genericDef == typeof(ICollection<>)
                            || genericDef == typeof(IList<>)
                            || genericDef == typeof(IReadOnlyList<>)
                            || genericDef == typeof(IReadOnlyCollection<>))
                        {
                            var listObj = (System.Collections.IList)Activator.CreateInstance(listType)!;
                            foreach (var item in list) listObj.Add(item);
                            pr.Property.SetValue(obj, listObj);
                        }
                        else
                        {
                            // Concrete generic collection: try to instantiate and add via ICollection<T>
                            var collectionInterface = typeof(ICollection<>).MakeGenericType(elementType);
                            if (collectionInterface.IsAssignableFrom(propType))
                            {
                                var collObj = Activator.CreateInstance(propType)!;
                                var addMethod = collectionInterface.GetMethod("Add")!;
                                foreach (var item in list) addMethod.Invoke(collObj, [item]);
                                pr.Property.SetValue(obj, collObj);
                            }
                            else
                            {
                                // Fallback: assign as List<T>
                                var listObj = (System.Collections.IList)Activator.CreateInstance(listType)!;
                                foreach (var item in list) listObj.Add(item);
                                pr.Property.SetValue(obj, listObj);
                            }
                        }
                    }
                }
            }

            // Process remaining (non-pipeline) args through root rules
            if (nonPipelineRules.Count > 0)
                ApplyRules(obj, nonPipelineRules, remainingArgs.ToArray(), ignoreUnknown);
            return;
        }

        args = ExpandCombinedShortFlags(args, rules);

        var setFieldNames = new HashSet<string>();
        var knownArgNames = BuildKnownArgNames(rules);
        var endOfOptionsIndex = Array.IndexOf(args, "--");
        var consumedAt = new Dictionary<string, int>();
        var positionalArgs = new List<(int index, string value)>();
        // Accumulates items for multi-value collection properties (string[], List<T>, T[], etc.)
        var pendingCollections = new Dictionary<string, (PropertyRule Rule, List<object> Items, int LastIndex)>();

        // Build a flat lookup: arg-name → (rule, forced bool for ValueForBool, enum member for enum flags)
        var argIndex = new Dictionary<string, (PropertyRule Rule, bool? BoolValue, EnumMemberRule? Member)>(StringComparer.Ordinal);
        foreach (var rule in rules)
        {
            if (rule.ValueForBoolTrueNames is not null && rule.ValueForBoolFalseNames is not null)
            {
                foreach (var n in rule.ValueForBoolTrueNames)  argIndex[n] = (rule, true,  null);
                foreach (var n in rule.ValueForBoolFalseNames) argIndex[n] = (rule, false, null);
            }
            else if (rule.HasParameterNames is not null && rule.HasParameterPosition < 0)
            {
                foreach (var n in rule.HasParameterNames) argIndex[n] = (rule, null, null);
            }
            else if (rule is { IsEnum: true, EnumMemberRules: not null, ValueForNames: null })
            {
                foreach (var mr in rule.EnumMemberRules)
                    if (mr.HasParameterNames is not null)
                        foreach (var n in mr.HasParameterNames) argIndex[n] = (rule, null, mr);
            }
            else if (rule.ValueForNames is not null)
            {
                foreach (var n in rule.ValueForNames) argIndex[n] = (rule, null, null);
            }
        }

        for (var i = 0; i < args.Length; i++)
        {
            var a = args[i];

            if (a == "--") continue;

            if (endOfOptionsIndex >= 0 && i > endOfOptionsIndex)
            {
                positionalArgs.Add((i, a));
                continue;
            }

            if (!a.StartsWith('-'))
            {
                positionalArgs.Add((i, a));
                continue;
            }

            // Derive the lookup key: for --key=value or -kVALUE strip the value part
            var key = a;
            string? inlineValue = null;

            var eqPos = a.IndexOf('=');
            if (eqPos > 0)
            {
                key = a[..eqPos];
                inlineValue = a[(eqPos + 1)..];
            }
            else if (a.Length > 2 && !a.StartsWith("--"))
            {
                // Could be -kVALUE — try both the full arg and the two-char prefix
                if (!argIndex.ContainsKey(a))
                {
                    var shortKey = a[..2];
                    if (argIndex.ContainsKey(shortKey))
                    {
                        key = shortKey;
                        inlineValue = a[2..];
                    }
                }
            }

            if (!argIndex.TryGetValue(key, out var entry))
            {
                if (!ignoreUnknown && !knownArgNames.Contains(key))
                    throw new ArgumentException($"Unknown argument: '{a}'.");
                continue;
            }

            var (rule, boolValue, member) = entry;

            // ── ArgsValueForBool ──────────────────────────────────────────────
            if (boolValue is not null)
            {
                SetTracked(rule.Property, boolValue.Value, i);
                continue;
            }

            // ── ArgsHasParameter (bool flag) ──────────────────────────────────
            if (member is null && rule.ValueForNames is null)
            {
                if ((Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType) == typeof(bool))
                    SetTracked(rule.Property, true, i);
                continue;
            }

            // ── ArgsEnum with per-member ArgsHasParameter ─────────────────────
            if (member is not null)
            {
                SetTracked(rule.Property, member.Value, i);
                continue;
            }

            // ── ArgsValueFor (and ArgsEnum backed by ArgsValueFor) ────────────
            var value = inlineValue;
            if (value is null)
            {
                var next = i + 1 < args.Length ? args[i + 1] : null;
                if (next is not null && !next.StartsWith('-'))
                {
                    value = next;
                    i++;
                }
                else if (next is not null && next.StartsWith('-') && !next.StartsWith("\"") && !next.StartsWith("'"))
                    throw new ArgumentException($"Argument '{key}' requires a value but got '{next}' instead.");
                else if (next is null && !rule.ValueForOptional)
                    throw new ArgumentException($"Argument '{key}' requires a value but none was provided.");
            }

            var propType = Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType;

            if (rule is { IsEnum: true, EnumMemberRules: not null })
            {
                if (value is not null)
                {
                    var enumMatched = false;
                    foreach (var mr in rule.EnumMemberRules)
                    {
                        var memberValue = mr.ArgsValue ?? mr.Value.ToString()!;
                        if (!string.Equals(memberValue, value, StringComparison.OrdinalIgnoreCase)) continue;
                        SetTracked(rule.Property, mr.Value, i);
                        enumMatched = true;
                        break;
                    }
                    if (!enumMatched)
                        throw new ArgumentException($"Invalid value '{value}' for argument '{key}'.");
                }
            }
            else if (value is not null && rule.ConvertorType is not null)
            {
                var convertorInstance = (IArgsConvertor)Activator.CreateInstance(rule.ConvertorType)!;
                var converted = convertorInstance.Convert(value);
                SetTracked(rule.Property, converted, i);
            }
            else if (value is not null && IsCollectionProperty(rule.Property.PropertyType, out var elementType))
            {
                // Multi-value collection: string[], T[], List<T>, HashSet<T>, etc.
                if (elementType == typeof(string)) ValidatePathConstraints(rule, value);
                var converted = rule.TupleDividers is not null && elementType.IsGenericType
                    ? ConvertTupleValue(value, elementType, rule.TupleDividers, rule.TuplePartsDividers)
                    : ConvertValue(value, elementType);
                if (!pendingCollections.TryGetValue(rule.Property.Name, out var pending))
                    pending = (rule, [], i);
                pending.Items.Add(converted);
                pendingCollections[rule.Property.Name] = (pending.Rule, pending.Items, i);
                setFieldNames.Add(rule.Property.Name);
                consumedAt[rule.Property.Name] = i;
            }
            else if (value is not null)
            {
                if (propType == typeof(string)) ValidatePathConstraints(rule, value);
                var converted = rule.TupleDividers is not null && propType.IsGenericType
                    ? ConvertTupleValue(value, propType, rule.TupleDividers, rule.TuplePartsDividers)
                    : ConvertValue(value, propType);
                SetTracked(rule.Property, converted, i);
            }
        }

        // ── Finalize pending collections ──────────────────────────────────────
        foreach (var (propName, (colRule, items, lastIdx)) in pendingCollections)
        {
            var finalValue = MaterializeCollection(colRule.Property.PropertyType, items);
            colRule.Property.SetValue(obj, finalValue);
        }

        // ── Post-processing: one pass over rules ─────────────────────────────
        var positionalValues = new HashSet<string>(positionalArgs.Select(p => p.value), StringComparer.OrdinalIgnoreCase);
        foreach (var rule in rules)
        {
            var name = rule.Property.Name;

            // Apply default for non-nullable enum properties not seen in args
            if (!setFieldNames.Contains(name)
                && rule.ValueForDefault is not null
                && rule.EnumMemberRules is not null
                && rule.ValueForNames is not null
                && Nullable.GetUnderlyingType(rule.Property.PropertyType) is null)
            {
                foreach (var mr in rule.EnumMemberRules)
                {
                    var memberValue = mr.ArgsValue ?? mr.Value.ToString()!;
                    if (!string.Equals(memberValue, rule.ValueForDefault, StringComparison.OrdinalIgnoreCase)) continue;
                    SetTracked(rule.Property, mr.Value, -1);
                    break;
                }
            }

            // ArgsHasParameter matched by positional value
            if (rule.HasParameterNames is not null
                && (rule.HasParameterPosition >= 0
                    || (rule.HasParameterPosition < 0 && rule.HasParameterNames.Any(n => !n.StartsWith('-')))))
            {
                var propType = Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType;
                if (propType == typeof(bool)
                    && rule.HasParameterNames.Any(positionalValues.Contains))
                    SetTracked(rule.Property, true, -1);
            }

            // ArgsPathspec
            if (rule.IsPathspec && endOfOptionsIndex >= 0)
            {
                var pathspecValues = args[(endOfOptionsIndex + 1)..];
                if (pathspecValues.Length > 0)
                    SetTracked(rule.Property, pathspecValues, -1);
            }

            // ArgsAfter
            if (rule.AfterFields is not null)
            {
                // Pre-assign any AfterFields that are implicit positionals not yet set,
                // reserving at least one positional for the ArgsAfter field itself.
                var implicitAfterFields = rule.AfterFields
                    .Where(f => !setFieldNames.Contains(f))
                    .Select(f => rules.FirstOrDefault(r => r.Property.Name == f && r.IsImplicitPositional))
                    .Where(r => r is not null)
                    .ToList();
                if (implicitAfterFields.Count > 0)
                {
                    var preConsumedIndices = new HashSet<int>(consumedAt.Values);
                    var availablePositionals = positionalArgs
                        .Where(p => !preConsumedIndices.Contains(p.index))
                        .ToList();
                    // Need at least one positional left for the ArgsAfter field itself
                    var canAssign = availablePositionals.Count - 1;
                    for (var pi = 0; pi < implicitAfterFields.Count && pi < canAssign; pi++)
                    {
                        var preRule = implicitAfterFields[pi]!;
                        var (pidx, pval) = availablePositionals[pi];
                        var prePropType = Nullable.GetUnderlyingType(preRule.Property.PropertyType) ?? preRule.Property.PropertyType;
                        if (prePropType == typeof(string)) ValidatePathConstraints(preRule, pval);
                        SetTracked(preRule.Property, ConvertValue(pval, prePropType), pidx);
                    }
                }

                // All specified fields must be set before the value can be assigned.
                // Implicit positional AfterFields are treated as optional: if there weren't
                // enough positional args to fill them, they are skipped rather than blocking.
                var unsatisfied = rule.AfterFields
                    .Where(f => !setFieldNames.Contains(f))
                    .Where(f => !rules.Any(r => r.Property.Name == f && r.IsImplicitPositional))
                    .ToList();
                if (unsatisfied.Count > 0)
                    continue;

                var minIndex = -1;
                foreach (var fieldName in rule.AfterFields)
                    if (consumedAt.TryGetValue(fieldName, out var idx) && idx > minIndex)
                        minIndex = idx;

                var candidate = positionalArgs.FirstOrDefault(p => p.index > minIndex);
                if (candidate.value is not null)
                {
                    // Ensure none of the AfterFields appear after the candidate (they become immutable)
                    foreach (var fieldName in rule.AfterFields)
                        if (consumedAt.TryGetValue(fieldName, out var fieldIdx) && fieldIdx > candidate.index)
                            throw new ArgumentException(
                                $"Field '{fieldName}' cannot be changed after '{rule.Property.Name}' has been assigned ([ArgsAfter]).");

                    var propType = Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType;
                    if (propType == typeof(string)) ValidatePathConstraints(rule, candidate.value);
                    SetTracked(rule.Property, ConvertValue(candidate.value, propType), candidate.index);
                }
            }
        }

        // ── Implicit positional assignment ────────────────────────────────────
        var implicitRules = rules.Where(r => r.IsImplicitPositional && !setFieldNames.Contains(r.Property.Name)).ToList();
        var consumedIndices = new HashSet<int>(consumedAt.Values);
        var unconsumedPositionals = positionalArgs
            .Where(p => !consumedIndices.Contains(p.index))
            .ToList();

        // ── ArgsPositional explicit assignment ───────────────────────────────
        var explicitPositionalRules = rules.Where(r => r.PositionalIndex >= 0).OrderBy(r => r.PositionalIndex).ToList();
        if (explicitPositionalRules.Count > 0)
        {
            // Validate: no duplicate positions
            var duplicates = explicitPositionalRules.GroupBy(r => r.PositionalIndex).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates.Count > 0)
                throw new ArgumentException($"Duplicate [ArgsPositional] index(es): {string.Join(", ", duplicates)}.");

            // Validate: must start at 0
            if (explicitPositionalRules[0].PositionalIndex != 0)
                throw new ArgumentException("[ArgsPositional] indices must start at 0.");

            // Validate: no gaps
            for (var pi = 1; pi < explicitPositionalRules.Count; pi++)
                if (explicitPositionalRules[pi].PositionalIndex != pi)
                    throw new ArgumentException($"[ArgsPositional] indices must be contiguous (missing index {pi}).");

            // Assign positional args to explicit positional rules
            for (var pi = 0; pi < explicitPositionalRules.Count && pi < unconsumedPositionals.Count; pi++)
            {
                var epRule = explicitPositionalRules[pi];
                var (pidx, pval) = unconsumedPositionals[pi];
                var propType = Nullable.GetUnderlyingType(epRule.Property.PropertyType) ?? epRule.Property.PropertyType;
                if (propType == typeof(string)) ValidatePathConstraints(epRule, pval);
                SetTracked(epRule.Property, ConvertValue(pval, propType), pidx);
            }
        }
        consumedIndices = new HashSet<int>(consumedAt.Values);
        var remainingPositionals = unconsumedPositionals
            .Where(p => !consumedIndices.Contains(p.index))
            .ToList();
        for (var pi = 0; pi < implicitRules.Count && pi < remainingPositionals.Count; pi++)
        {
            var ipRule = implicitRules[pi];
            var (pidx, pval) = remainingPositionals[pi];
            var propType = Nullable.GetUnderlyingType(ipRule.Property.PropertyType) ?? ipRule.Property.PropertyType;
            if (propType == typeof(string)) ValidatePathConstraints(ipRule, pval);
            SetTracked(ipRule.Property, ConvertValue(pval, propType), pidx);
        }

        // ── Remaining validation ──────────────────────────────────────────────
        foreach (var rule in rules)
        {
            var name = rule.Property.Name;

            // ArgsOneOf validation
            if (rule.OneOfFields is not null && setFieldNames.Contains(name))
            {
                var alsoSet = rule.OneOfFields.Where(setFieldNames.Contains).ToList();
                if (alsoSet.Count > 0)
                    throw new InvalidOperationException(
                        $"Property '{name}' and '{string.Join("', '", alsoSet)}' are mutually exclusive ([ArgsOneOf]).");
            }

            // ArgsIfSet validation
            if (rule.IfSetFields is not null && setFieldNames.Contains(name))
            {
                foreach (var required in rule.IfSetFields)
                    if (!setFieldNames.Contains(required))
                        throw new ArgumentException(
                            $"Property '{name}' requires '{required}' to be set.");
            }
        }

        return;

        void SetTracked(PropertyInfo prop, object value, int argIndx)
        {
            if (setFieldNames.Contains(prop.Name))
                throw new ArgumentException($"Argument for '{prop.Name}' was specified more than once.");

            prop.SetValue(obj, value);
            setFieldNames.Add(prop.Name);
            if (argIndx >= 0)
                consumedAt[prop.Name] = argIndx;
        }
    }

    private static HashSet<string> BuildKnownArgNames(List<PropertyRule> rules)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rule in rules)
        {
            if (rule.HasParameterNames is not null)
                foreach (var n in rule.HasParameterNames) names.Add(n);
            if (rule.ValueForNames is not null)
                foreach (var n in rule.ValueForNames) names.Add(n);
            if (rule.ValueForBoolTrueNames is not null)
                foreach (var n in rule.ValueForBoolTrueNames) names.Add(n);
            if (rule.ValueForBoolFalseNames is not null)
                foreach (var n in rule.ValueForBoolFalseNames) names.Add(n);
            if (rule.EnumMemberRules is not null)
                foreach (var mr in rule.EnumMemberRules)
                    if (mr.HasParameterNames is not null)
                        foreach (var n in mr.HasParameterNames) names.Add(n);
        }
        return names;
    }

    // Expand combined short flags: -am → -a -m (last flag can have a value)
    private static string[] ExpandCombinedShortFlags(string[] args, List<PropertyRule> rules)
    {
        // Collect all single-char flag names
        var singleCharFlags = new HashSet<char>();
        var valueFlags = new HashSet<char>(); // flags that take a value

        foreach (var rule in rules)
        {
            CollectSingleCharFlags(rule.HasParameterNames, singleCharFlags, null);
            CollectSingleCharFlags(rule.ValueForNames, singleCharFlags, valueFlags);
            CollectSingleCharFlags(rule.ValueForBoolTrueNames, singleCharFlags, null);
            CollectSingleCharFlags(rule.ValueForBoolFalseNames, singleCharFlags, null);
            if (rule.EnumMemberRules is not null)
                foreach (var mr in rule.EnumMemberRules)
                    CollectSingleCharFlags(mr.HasParameterNames, singleCharFlags, null);
        }

        if (singleCharFlags.Count == 0 && valueFlags.Count == 0)
            return args;

        var result = new List<string>(args.Length);
        var anyExpanded = false;
        foreach (var arg in args)
        {
            // Combined short flags: start with '-', not '--', length > 2
            if (arg.StartsWith('-') && !arg.StartsWith("--") && arg.Length > 2 && !arg.Contains('='))
            {
                var chars = arg[1..];
                // Check all chars are known single-char flags
                var allKnown = chars.All(c => singleCharFlags.Contains(c) || valueFlags.Contains(c));
                if (allKnown && chars.Length > 1)
                {
                    anyExpanded = true;
                    for (var ci = 0; ci < chars.Length - 1; ci++)
                        result.Add($"-{chars[ci]}");
                    // Last char: if it's a value flag, add it alone (value comes next)
                    result.Add($"-{chars[^1]}");
                }
                else result.Add(arg);
            }
            else
            {
                result.Add(arg);
            }
        }
        return anyExpanded ? result.ToArray() : args;

        static void CollectSingleCharFlags(string[]? names, HashSet<char> flags, HashSet<char>? valueSet)
        {
            if (names is null) return;
            foreach (var n in names)
                if (n is ['-', _])
                {
                    flags.Add(n[1]);
                    valueSet?.Add(n[1]);
                }
        }
    }

    private static object ConvertTupleValue(string raw, Type tupleType, string[] dividers, bool partsDividers)
    {
        if (raw is ['"', _, ..] && raw[^1] == '"')
            raw = raw[1..^1];

        var typeArgs = tupleType.GetGenericArguments();
        var parts = new string[typeArgs.Length];
        var remaining = raw;
        for (var i = 0; i < typeArgs.Length - 1; i++)
        {
            if (partsDividers)
            {
                var divider = dividers[i];
                var idx = remaining.IndexOf(divider, StringComparison.Ordinal);
                if (idx < 0)
                    throw new ArgumentException($"Expected divider '{divider}' in value '{raw}'.");
                parts[i] = remaining[..idx];
                remaining = remaining[(idx + divider.Length)..];
            }
            else
            {
                // Try each divider as an alternative
                var bestIdx = -1;
                var bestDivider = dividers[i % dividers.Length];
                foreach (var d in dividers)
                {
                    var idx = remaining.IndexOf(d, StringComparison.Ordinal);
                    if (idx >= 0 && (bestIdx < 0 || idx < bestIdx))
                    {
                        bestIdx = idx;
                        bestDivider = d;
                    }
                }
                if (bestIdx < 0)
                    throw new ArgumentException($"Expected one of dividers [{string.Join(", ", dividers.Select(d => $"'{d}'"))}] in value '{raw}'.");
                parts[i] = remaining[..bestIdx];
                remaining = remaining[(bestIdx + bestDivider.Length)..];
            }
        }
        parts[typeArgs.Length - 1] = remaining;

        var values = new object[typeArgs.Length];
        for (var i = 0; i < typeArgs.Length; i++)
            values[i] = ConvertValue(parts[i], typeArgs[i]);

        return Activator.CreateInstance(tupleType, values)!;
    }

    private static object ConvertValue(string raw, Type targetType)
    {
        // Strip surrounding quotes
        if (raw is ['"', _, ..] && raw[^1] == '"')
            raw = raw[1..^1];

        try
        {
            if (targetType == typeof(string)) return raw;
            if (targetType == typeof(bool)) return bool.Parse(raw);
            if (targetType == typeof(int)) return int.Parse(raw);
            if (targetType == typeof(DateTime)) return DateTime.Parse(raw);
            if (targetType.IsEnum) return Enum.Parse(targetType, raw, ignoreCase: true);
            return Convert.ChangeType(raw, targetType);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException)
        {
            throw new ArgumentException($"Invalid value '{raw}' for type '{targetType.Name}'.", ex);
        }
    }

    private static bool IsCollectionProperty(Type type, out Type elementType)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        // Array: T[]
        if (underlying.IsArray)
        {
            elementType = underlying.GetElementType()!;
            return true;
        }

        // Generic collection: List<T>, IList<T>, HashSet<T>, Queue<T>, IEnumerable<T>, etc.
        if (underlying.IsGenericType)
        {
            var typeArg = underlying.GetGenericArguments()[0];
            var collInterface = typeof(ICollection<>).MakeGenericType(typeArg);
            var enumInterface = typeof(IEnumerable<>).MakeGenericType(typeArg);
            if (collInterface.IsAssignableFrom(underlying) || enumInterface.IsAssignableFrom(underlying))
            {
                elementType = typeArg;
                return true;
            }
        }

        elementType = type;
        return false;
    }

    private static object MaterializeCollection(Type propType, List<object> items)
    {
        var underlying = Nullable.GetUnderlyingType(propType) ?? propType;

        // Array: T[]
        if (underlying.IsArray)
        {
            var elementType = underlying.GetElementType()!;
            var array = Array.CreateInstance(elementType, items.Count);
            for (var i = 0; i < items.Count; i++)
                array.SetValue(items[i], i);
            return array;
        }

        if (!underlying.IsGenericType)
            throw new InvalidOperationException($"Cannot materialize collection for type '{propType.Name}'.");

        var typeArg = underlying.GetGenericArguments()[0];
        var def = underlying.GetGenericTypeDefinition();

        // Interface types → materialise as List<T>
        if (def == typeof(IEnumerable<>) || def == typeof(ICollection<>) || def == typeof(IList<>)
            || def == typeof(IReadOnlyList<>) || def == typeof(IReadOnlyCollection<>) || def == typeof(ISet<>)
            || def == typeof(IReadOnlySet<>))
        {
            var fallbackType = (def == typeof(ISet<>) || def == typeof(IReadOnlySet<>))
                ? typeof(HashSet<>).MakeGenericType(typeArg)
                : typeof(List<>).MakeGenericType(typeArg);
            return FillCollection(fallbackType, typeArg, items);
        }

        return FillCollection(underlying, typeArg, items);
    }

    private static void ValidatePathConstraints(PropertyRule rule, string value)
    {
        if (rule.IsExistingOnlyFile)
        {
            if (!File.Exists(value))
                throw new ArgumentException($"File '{value}' does not exist (property '{rule.Property.Name}').");
        }
        if (rule.IsExistingOnlyDirectory)
        {
            if (!Directory.Exists(value))
                throw new ArgumentException($"Directory '{value}' does not exist (property '{rule.Property.Name}').");
        }
        if (rule.IsLegalFileNamesOnly)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            if (value.IndexOfAny(invalidChars) >= 0)
                throw new ArgumentException($"Value '{value}' contains illegal file name characters (property '{rule.Property.Name}').");
        }
        if (rule.AcceptFromAmong is not null)
        {
            if (!rule.AcceptFromAmong.Contains(value, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Value '{value}' is not accepted for property '{rule.Property.Name}'. Accepted values: {string.Join(", ", rule.AcceptFromAmong)}.");
        }
    }

    private static object FillCollection(Type collectionType, Type elementType, List<object> items)
    {
        var instance = Activator.CreateInstance(collectionType)!;
        var addMethod = typeof(ICollection<>).MakeGenericType(elementType).GetMethod("Add")
            ?? collectionType.GetMethod("Add", [elementType])
            ?? collectionType.GetMethod("Enqueue", [elementType]);
        if (addMethod is null)
            throw new InvalidOperationException($"Cannot find Add/Enqueue method on '{collectionType.Name}'.");
        foreach (var item in items)
            addMethod.Invoke(instance, [item]);
        return instance;
    }
}
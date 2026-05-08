using ArgsToConfig.Attributes;
using ArgsToConfig.Models;
using System.Reflection;

namespace ArgsToConfig;

internal static class InnerToObject
{
    private static readonly Dictionary<Type, List<PropertyRule>> RulesCache = new();

    internal static List<PropertyRule> BuildRules(Type type)
    {
        if (!RulesCache.TryGetValue(type, out var rules))
        {
            rules = BuildRulesCore(type);
            RulesCache[type] = rules;
        }
        return rules;
    }

    private static List<PropertyRule> BuildRulesCore(Type type)
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
                    ObjectRootName = isObject.GetNames
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
                var enumType = UnwrapNullable(prop.PropertyType);
                if (enumType.IsEnum)
                {
                    var members = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
                    enumMemberRules = members.Select(m =>
                    {
                        var mVal = m.GetCustomAttribute<ArgsValueAttribute>();
                        return new EnumMemberRule
                        {
                            Value = m.GetValue(null)!,
                            ArgsValue = mVal?.GetValues
                        };
                    }).ToArray();

                    // Promote ArgsEnum name params to valueFor if specified on the attribute
                    if (argsEnum!.GetNames is not null && valueFor is null)
                        valueFor = new ArgsValueForAttribute(string.Join("|", argsEnum.GetNames))
                        {
                            Optional = argsEnum.Optional,
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
                ValueForOptional = valueFor?.Optional ?? false,
                ValueForDefault = valueFor?.DefaultValue,
                ValueForBoolTrueNames = valueForBool?.GetTrueNames,
                ValueForBoolFalseNames = valueForBool?.GetFalseNames,
                IsEnum = isEnum,
                AfterFields = after?.GetFields,
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
                AcceptFromAmong = acceptFromAmong?.GetValues,
                EnvVar = hasParam?.EnvVar ?? valueFor?.EnvVar ?? argsEnum?.EnvVar
            });
        }

        return rules;
    }

    internal static (string? error, int? position) ApplyRules(object obj, List<PropertyRule> rules, string[] args, bool ignoreUnknown = false)
    {
        // ── ArgsObject subcommand dispatch ───────────────────────────────────
        var objectRules = rules.Where(r => r.IsObject).ToList();
        if (objectRules.Count > 0)
        {
            // Build a map: subcommand name → rule (supports pipe-separated and dash-prefixed names)
            var subcmdMap = new Dictionary<string, PropertyRule>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in objectRules)
                if (r.ObjectRootName is not null)
                    foreach (var n in r.ObjectRootName)
                        subcmdMap[n.Trim()] = r;

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
                var subType2 = UnwrapNullable(subRule.Property.PropertyType);
                var subArgNames2 = BuildKnownArgNames(GetOrBuildRules(subRulesCache, subType2));
                var conflicts2 = parentArgNames.Intersect(subArgNames2, StringComparer.OrdinalIgnoreCase).ToList();
                if (conflicts2.Count > 0)
                {
                    var conflictArgIndex = conflicts2.Select(c => Array.IndexOf(args, c)).Where(idx => idx >= 0).DefaultIfEmpty(0).Max();
                    return ($"Argument name conflict between root and subcommand '{subcmdName2}': {string.Join(", ", conflicts2)}.", conflictArgIndex);
                }
            }

            // Segment args by subcommand keywords; non-subcommand args go to the root
            // Each segment: (rule, startIndex, args slice)
            var segments = new List<(PropertyRule Rule, List<string> SegArgs)>();
            List<string>? currentSegArgs = null;
            var rootArgs = new List<string>();

            foreach (var a in args)
            {
                if (subcmdMap.TryGetValue(a, out var foundRule))
                {
                    // Start a new segment for this subcommand (works for both dash and non-dash names)
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
                return ("No subcommand specified.", 0);

            // Apply each subcommand segment
            foreach (var (segRule, segArgs) in segments)
            {
                var subType = UnwrapNullable(segRule.Property.PropertyType);
                var subObj = Activator.CreateInstance(subType)!;
                var subRules = GetOrBuildRules(subRulesCache, subType);
                var (subErr, subPos) = ApplyRules(subObj, subRules, segArgs.ToArray());
                if (subErr is not null) return (subErr, subPos);
                segRule.Property.SetValue(obj, subObj);
            }

            // Process non-object rules against root args (args before any subcommand)
            if (nonObjectRules.Count > 0)
            {
                var (nonObjErr, nonObjPos) = ApplyRules(obj, nonObjectRules, rootArgs.ToArray(), ignoreUnknown: true);
                if (nonObjErr is not null) return (nonObjErr, nonObjPos);
            }
            return (null, null);
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
                    return ($"Multiple [ArgsPipeline] properties use the same interface '{iface.Name}'. Collections in the root level must be based on different interfaces.", null);
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
                {
                    var dupIdx = Array.IndexOf(args, cmdName);
                    return ($"Duplicate [ArgsPipelineCommand] name '{cmdName}' found across pipeline properties.", dupIdx >= 0 ? dupIdx : 0);
                }
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
            {
                var conflictIdx = Array.IndexOf(args, cmdName);
                return ($"Pipeline command name '{cmdName}' conflicts with a root parameter name.", conflictIdx >= 0 ? conflictIdx : 0);
            }

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
                    {
                        var otherIdx = Array.IndexOf(args, otherCmdName);
                        return ($"Pipeline command '{cmdName}' uses another pipeline command name '{otherCmdName}' as an argument.", otherIdx >= 0 ? otherIdx : 0);
                    }
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
                        return ($"Cannot go back to a previously used pipeline. Command '{a}' belongs to a pipeline that was already closed.", i2);
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
                    var cmdRules = GetOrBuildRules(cmdRulesCache, cmdEntry.Type);
                    var (cmdErr, cmdPos) = ApplyRules(cmdObj, cmdRules, cmdArgsList.ToArray());
                    if (cmdErr is not null) return (cmdErr, cmdPos);
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
                            foreach (var item in list)
                                listObj.Add(item);
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
                                foreach (var item in list)
                                    addMethod.Invoke(collObj, [item]);
                                pr.Property.SetValue(obj, collObj);
                            }
                            else
                            {
                                // Fallback: assign as List<T>
                                var listObj = (System.Collections.IList)Activator.CreateInstance(listType)!;
                                foreach (var item in list)
                                    listObj.Add(item);
                                pr.Property.SetValue(obj, listObj);
                            }
                        }
                    }
                }
            }

            // Process remaining (non-pipeline) args through root rules
            if (nonPipelineRules.Count > 0)
            {
                var (nonPipeErr, nonPipePos) = ApplyRules(obj, nonPipelineRules, remainingArgs.ToArray(), ignoreUnknown);
                if (nonPipeErr is not null) return (nonPipeErr, nonPipePos);
            }
            return (null, null);
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
                foreach (var n in rule.ValueForBoolTrueNames)
                    argIndex[n] = (rule, true, null);
                foreach (var n in rule.ValueForBoolFalseNames)
                    argIndex[n] = (rule, false, null);
            }
            else if (rule.HasParameterNames is not null && rule.HasParameterPosition < 0)
            {
                foreach (var n in rule.HasParameterNames)
                    argIndex[n] = (rule, null, null);
            }
            else if (rule is { IsEnum: true, EnumMemberRules: not null, ValueForNames: null })
            {
                foreach (var mr in rule.EnumMemberRules)
                {
                    // Also register dash-prefixed ArgsValue strings as direct flag entries
                    if (mr.ArgsValue is not null)
                        foreach (var n in mr.ArgsValue)
                            if (n.Trim().StartsWith('-'))
                                argIndex[n.Trim()] = (rule, null, mr);
                }
            }
            else if (rule.ValueForNames is not null)
            {
                foreach (var n in rule.ValueForNames)
                    argIndex[n] = (rule, null, null);
            }
        }

        for (var i = 0; i < args.Length; i++)
        {
            var a = args[i];

            if (a == "--")
                continue;

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
                        return ($"Unknown argument: '{a}'.", i);
                continue;
            }

            var (rule, boolValue, member) = entry;

            // ── ArgsValueForBool ──────────────────────────────────────────────
            if (boolValue is not null)
            {
                var e4 = SetTracked(rule.Property, boolValue.Value, i);
                if (e4 is not null) return (e4, i);
                continue;
            }

            // ── ArgsHasParameter (bool flag) ──────────────────────────────────
            if (member is null && rule.ValueForNames is null)
            {
                if (UnwrapNullable(rule.Property.PropertyType) == typeof(bool))
                {
                    var e3 = SetTracked(rule.Property, true, i);
                    if (e3 is not null) return (e3, i);
                }
                continue;
            }

            // ── ArgsEnum with per-member ArgsHasParameter ─────────────────────
            if (member is not null)
            {
                var e5 = SetTracked(rule.Property, member.Value, i);
                if (e5 is not null) return (e5, i);
                continue;
            }

            // ── ArgsValueFor (and ArgsEnum backed by ArgsValueFor) ────────────
            var flagIndex = i;
            var value = inlineValue;
            if (value is null)
            {
                var next = i + 1 < args.Length ? args[i + 1] : null;
                if (next is not null && !next.StartsWith('-'))
                {
                    value = next;
                    i++;
                }
                if (next is not null && next.StartsWith('-') && !next.StartsWith("\"") && !next.StartsWith("'"))
                    return ($"Argument '{key}' requires a value but got '{next}' instead.", i + 1);
                else if (next is null && !rule.ValueForOptional)
                    return ($"Argument '{key}' requires a value but none was provided.", flagIndex);
            }

            var propType = UnwrapNullable(rule.Property.PropertyType);

            if (rule is { IsEnum: true, EnumMemberRules: not null })
            {
                {
                    var enumMatched = false;
                    foreach (var mr in rule.EnumMemberRules)
                    {
                        var matched = mr.ArgsValue?.Any(v => string.Equals(v, value, StringComparison.OrdinalIgnoreCase)) ?? 
                                      string.Equals(mr.Value.ToString(), value, StringComparison.OrdinalIgnoreCase);
                        if (!matched)
                            continue;
                        var e2 = SetTracked(rule.Property, mr.Value, flagIndex);
                        if (e2 is not null) return (e2, flagIndex);
                        enumMatched = true;
                        break;
                    }
                    if (!enumMatched)
                        return ($"Invalid value '{value}' for argument '{key}'.", i);
                }
            }
            else if (value is not null && rule.ConvertorType is not null)
            {
                try
                {
                    var convertorInstance = (IArgsConvertor)Activator.CreateInstance(rule.ConvertorType)!;
                    var converted = convertorInstance.Convert(value);
                    var e0 = SetTracked(rule.Property, converted, flagIndex);
                    if (e0 is not null) return (e0, flagIndex);
                }
                catch (Exception ex)
                {
                    return (ex.Message, i);
                }
            }
            else if (value is not null && IsCollectionProperty(rule.Property.PropertyType, out var elementType))
            {
                // Multi-value collection: string[], T[], List<T>, HashSet<T>, etc.
                if (elementType == typeof(string))
                {
                    var pathErr = ValidateValueConstraints(rule, value);
                    if (pathErr is not null) return (pathErr, i);
                }
                object converted;
                if (rule.TupleDividers is not null && elementType.IsGenericType)
                {
                    var (tupleErr, tupleResult) = ConvertTupleValue(value, elementType, rule.TupleDividers, rule.TuplePartsDividers);
                    if (tupleErr is not null) return (tupleErr, i);
                    converted = tupleResult!;
                }
                else
                {
                    var (convErr, convResult) = ConvertValue(value, elementType);
                    if (convErr is not null) return (convErr, i);
                    converted = convResult!;
                }
                if (!pendingCollections.TryGetValue(rule.Property.Name, out var pending))
                    pending = (rule, [], flagIndex);
                pending.Items.Add(converted);
                pendingCollections[rule.Property.Name] = (pending.Rule, pending.Items, flagIndex);
                setFieldNames.Add(rule.Property.Name);
                consumedAt[rule.Property.Name] = flagIndex;
            }
            else if (value is not null)
            {
                if (propType == typeof(string))
                {
                    var pathErr = ValidateValueConstraints(rule, value);
                    if (pathErr is not null) return (pathErr, i);
                }
                object converted;
                if (rule.TupleDividers is not null && propType.IsGenericType)
                {
                    var (tupleErr, tupleResult) = ConvertTupleValue(value, propType, rule.TupleDividers, rule.TuplePartsDividers);
                    if (tupleErr is not null) return (tupleErr, i);
                    converted = tupleResult!;
                }
                else
                {
                    var (convErr, convResult) = ConvertValue(value, propType);
                    if (convErr is not null) return (convErr, i);
                    converted = convResult!;
                }
                var e1 = SetTracked(rule.Property, converted, flagIndex);
                if (e1 is not null) return (e1, flagIndex);
            }
        }

        // ── Finalize pending collections
        foreach (var (_, (colRule, items, _)) in pendingCollections)
        {
            var (matErr, matResult) = MaterializeCollection(colRule.Property.PropertyType, items);
            if (matErr is not null) return (matErr, null);
            colRule.Property.SetValue(obj, matResult);
        }

        // ── Environment variable fallback ─────────────────────────────────────
        var envVarRules = rules.Where(r => r.EnvVar is not null && !setFieldNames.Contains(r.Property.Name)).ToList();
        if (envVarRules.Count > 0)
        {
            var dotEnv = LoadDotEnv();
            foreach (var rule in envVarRules)
            {
                var envValue = Environment.GetEnvironmentVariable(rule.EnvVar!)
                    ?? dotEnv.GetValueOrDefault(rule.EnvVar!);
                if (envValue is null)
                    continue;

                var propType2 = UnwrapNullable(rule.Property.PropertyType);

                // ArgsHasParameter: treat env var value as a bool (true/false/1/0 or empty = false)
                if (rule.HasParameterNames is not null && rule.ValueForNames is null && rule.EnumMemberRules is null)
                {
                    if (propType2 != typeof(bool))
                        continue;
                    var boolResult = !string.IsNullOrEmpty(envValue) && envValue is "1" or "true" or "True" or "TRUE";
                    var eb = SetTracked(rule.Property, boolResult, -1);
                    if (eb is not null) return (eb, null);
                    continue;
                }

                // ArgsEnum: match enum members
                if (rule is { IsEnum: true, EnumMemberRules: not null })
                {
                    var enumMatched = false;
                    foreach (var mr in rule.EnumMemberRules)
                    {
                        var matched = mr.ArgsValue?.Any(v => string.Equals(v, envValue, StringComparison.OrdinalIgnoreCase)) ?? 
                                      string.Equals(mr.Value.ToString(), envValue, StringComparison.OrdinalIgnoreCase);
                        if (!matched)
                            continue;
                        var ee = SetTracked(rule.Property, mr.Value, -1);
                        if (ee is not null) return (ee, null);
                        enumMatched = true;
                        break;
                    }
                    if (!enumMatched)
                        return ($"Invalid value '{envValue}' for environment variable '{rule.EnvVar}'.", null);
                    continue;
                }

                // ArgsValueFor: convert normally
                {
                    if (propType2 == typeof(string))
                    {
                        var pathErr = ValidateValueConstraints(rule, envValue);
                        if (pathErr is not null) return (pathErr, null);
                    }
                    var (convErr, convResult) = ConvertValue(envValue, propType2);
                    if (convErr is not null) return (convErr, null);
                    var ev = SetTracked(rule.Property, convResult!, -1);
                    if (ev is not null) return (ev, null);
                }
            }
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
                    var defaultMatched = mr.ArgsValue?.Any(v => string.Equals(v, rule.ValueForDefault, StringComparison.OrdinalIgnoreCase)) ?? 
                                         string.Equals(mr.Value.ToString(), rule.ValueForDefault, StringComparison.OrdinalIgnoreCase);
                    if (!defaultMatched)
                        continue;
                    var e6 = SetTracked(rule.Property, mr.Value, -1);
                    if (e6 is not null) return (e6, null);
                    break;
                }
            }

            // ArgsHasParameter matched by positional value
            if (rule.HasParameterNames is not null
                && (rule.HasParameterPosition >= 0
                    || (rule.HasParameterPosition < 0 && rule.HasParameterNames.Any(n => !n.StartsWith('-')))))
            {
                var propType = UnwrapNullable(rule.Property.PropertyType);
                if (propType == typeof(bool)
                    && rule.HasParameterNames.Any(positionalValues.Contains))
                {
                    var e7 = SetTracked(rule.Property, true, -1);
                    if (e7 is not null) return (e7, null);
                }
            }

            // Unnamed ArgsEnum: match non-dash ArgsValue strings against positional args
            if (!setFieldNames.Contains(name)
                && rule is { IsEnum: true, EnumMemberRules: not null, ValueForNames: null })
            {
                var matched = false;
                foreach (var positional in positionalArgs)
                {
                    foreach (var mr in rule.EnumMemberRules)
                    {
                        if (mr.ArgsValue is null) continue;
                        if (!mr.ArgsValue.Any(v => !v.Trim().StartsWith('-')
                               && string.Equals(v.Trim(), positional.value, StringComparison.OrdinalIgnoreCase))) continue;
                        var ep = SetTracked(rule.Property, mr.Value, positional.index);
                        if (ep is not null) return (ep, positional.index);
                        matched = true;
                        break;
                    }
                    if (matched) break;
                }
            }

            // ArgsPathspec
            if (rule.IsPathspec && endOfOptionsIndex >= 0)
            {
                var pathspecValues = args[(endOfOptionsIndex + 1)..];
                if (pathspecValues.Length > 0)
                {
                    var e8 = SetTracked(rule.Property, pathspecValues, -1);
                    if (e8 is not null) return (e8, null);
                }
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
                        var prePropType = UnwrapNullable(preRule.Property.PropertyType);
                        if (prePropType == typeof(string))
                        {
                            var pathErr = ValidateValueConstraints(preRule, pval);
                            if (pathErr is not null) return (pathErr, pidx);
                        }
                        var (convErr, convResult) = ConvertValue(pval, prePropType);
                        if (convErr is not null) return (convErr, pidx);
                        var e9 = SetTracked(preRule.Property, convResult!, pidx);
                        if (e9 is not null) return (e9, pidx);
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
                            return ($"Field '{fieldName}' cannot be changed after '{rule.Property.Name}' has been assigned ([ArgsAfter]).", candidate.index);

                    var propType = UnwrapNullable(rule.Property.PropertyType);
                    if (propType == typeof(string))
                    {
                        var pathErr = ValidateValueConstraints(rule, candidate.value);
                        if (pathErr is not null) return (pathErr, candidate.index);
                    }
                    var (convErr, convResult) = ConvertValue(candidate.value, propType);
                    if (convErr is not null) return (convErr, candidate.index);
                    var e10 = SetTracked(rule.Property, convResult!, candidate.index);
                    if (e10 is not null) return (e10, candidate.index);
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
                return ($"Duplicate [ArgsPositional] index(es): {string.Join(", ", duplicates)}.", null);

            // Validate: must start at 0
            if (explicitPositionalRules[0].PositionalIndex != 0)
                return ("[ArgsPositional] indices must start at 0.", null);

            // Validate: no gaps
            for (var pi = 1; pi < explicitPositionalRules.Count; pi++)
                if (explicitPositionalRules[pi].PositionalIndex != pi)
                    return ($"[ArgsPositional] indices must be contiguous (missing index {pi}).", null);

            // Assign positional args to explicit positional rules
            for (var pi = 0; pi < explicitPositionalRules.Count && pi < unconsumedPositionals.Count; pi++)
            {
                var epRule = explicitPositionalRules[pi];
                var (pidx, pval) = unconsumedPositionals[pi];
                var propType = UnwrapNullable(epRule.Property.PropertyType);
                if (propType == typeof(string))
                {
                    var pathErr = ValidateValueConstraints(epRule, pval);
                    if (pathErr is not null) return (pathErr, pidx);
                }
                var (convErr, convResult) = ConvertValue(pval, propType);
                if (convErr is not null) return (convErr, pidx);
                var e11 = SetTracked(epRule.Property, convResult!, pidx);
                if (e11 is not null) return (e11, pidx);
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
            var propType = UnwrapNullable(ipRule.Property.PropertyType);
            if (propType == typeof(string))
            {
                var pathErr = ValidateValueConstraints(ipRule, pval);
                if (pathErr is not null) return (pathErr, pidx);
            }
            var (convErr, convResult) = ConvertValue(pval, propType);
            if (convErr is not null) return (convErr, pidx);
            var e12 = SetTracked(ipRule.Property, convResult!, pidx);
            if (e12 is not null) return (e12, pidx);
        }

        // ── Remaining validation

        // ArgsOneOf class-level validation
        var oneOfAttributes = obj.GetType().GetCustomAttributes<ArgsOneOfAttribute>();
        foreach (var oneOfAttr in oneOfAttributes)
        {
            var setInGroup = oneOfAttr.GetFields.Where(setFieldNames.Contains).ToList();
            if (setInGroup.Count > 1)
            {
                var lastConflictPos = setInGroup
                    .Where(consumedAt.ContainsKey)
                    .Select(f => consumedAt[f])
                    .DefaultIfEmpty(0)
                    .Max();
                return ($"Properties '{string.Join("', '", oneOfAttr.GetFields)}' are mutually exclusive ([ArgsOneOf]).", lastConflictPos);
            }
        }

        foreach (var rule in rules)
        {
            var name = rule.Property.Name;

            // ArgsIfSet validation
            if (rule.IfSetFields is not null && setFieldNames.Contains(name))
            {
                foreach (var required in rule.IfSetFields)
                    if (!setFieldNames.Contains(required))
                    {
                        var ifSetPos = consumedAt.GetValueOrDefault(name, 0);
                        return ($"Property '{name}' requires '{required}' to be set.", ifSetPos);
                    }
            }
        }

        return (null, null);

        string? SetTracked(PropertyInfo prop, object value, int argIndx)
        {
            if (setFieldNames.Contains(prop.Name))
                return $"Argument for '{prop.Name}' was specified more than once.";

            prop.SetValue(obj, value);
            setFieldNames.Add(prop.Name);
            if (argIndx >= 0)
                consumedAt[prop.Name] = argIndx;
            return null;
        }
    }

    private static Type UnwrapNullable(Type t) => Nullable.GetUnderlyingType(t) ?? t;

    private static List<PropertyRule> GetOrBuildRules(Dictionary<Type, List<PropertyRule>> cache, Type type) =>
        cache.TryGetValue(type, out var r) ? r : cache[type] = BuildRules(type);

    private static HashSet<string> BuildKnownArgNames(List<PropertyRule> rules)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rule in rules)
        {
            if (rule.HasParameterNames is not null)
                foreach (var n in rule.HasParameterNames)
                    names.Add(n);
            if (rule.ValueForNames is not null)
                foreach (var n in rule.ValueForNames)
                    names.Add(n);
            if (rule.ValueForBoolTrueNames is not null)
                foreach (var n in rule.ValueForBoolTrueNames)
                    names.Add(n);
            if (rule.ValueForBoolFalseNames is not null)
                foreach (var n in rule.ValueForBoolFalseNames)
                    names.Add(n);
            if (rule.EnumMemberRules is not null)
                foreach (var mr in rule.EnumMemberRules)
                {
                    if (mr.ArgsValue is not null && rule.ValueForNames is null)
                        foreach (var n in mr.ArgsValue)
                            if (n.Trim().StartsWith('-'))
                                names.Add(n.Trim());
                }
        }
        return names;
    }

    // Expand combined short flags: -am → -a -m (last flag can have a value)
    private static string[] ExpandCombinedShortFlags(string[] args, List<PropertyRule> rules)
    {
        // Collect all single-char flag names
        var singleCharFlags = new HashSet<char>();

        foreach (var rule in rules)
        {
            CollectSingleCharFlags(rule.HasParameterNames, singleCharFlags, null);
            CollectSingleCharFlags(rule.ValueForNames, singleCharFlags, null);
            CollectSingleCharFlags(rule.ValueForBoolTrueNames, singleCharFlags, null);
            CollectSingleCharFlags(rule.ValueForBoolFalseNames, singleCharFlags, null);
            if (rule.EnumMemberRules is not null)
                foreach (var mr in rule.EnumMemberRules)
                {
                    // ArgsValue dash alternatives on unnamed enum members
                    if (mr.ArgsValue is not null && rule.ValueForNames is null)
                        CollectSingleCharFlags(mr.ArgsValue.Select(v => v.Trim()).ToArray(), singleCharFlags, null);
                }
        }

        if (singleCharFlags.Count == 0)
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
                var allKnown = chars.All(singleCharFlags.Contains);
                if (allKnown && chars.Length > 1)
                {
                    anyExpanded = true;
                    for (var ci = 0; ci < chars.Length - 1; ci++)
                        result.Add($"-{chars[ci]}");
                    // Last char: if it's a value flag, add it alone (value comes next)
                    result.Add($"-{chars[^1]}");
                }
                else
                    result.Add(arg);
            }
            else
            {
                result.Add(arg);
            }
        }
        return anyExpanded ? result.ToArray() : args;

        static void CollectSingleCharFlags(string[]? names, HashSet<char> flags, HashSet<char>? valueSet)
        {
            if (names is null)
                return;
            foreach (var n in names)
                if (n is ['-', _])
                {
                    flags.Add(n[1]);
                    valueSet?.Add(n[1]);
                }
        }
    }

    private static (string? error, object? result) ConvertTupleValue(string raw, Type tupleType, string[] dividers, bool partsDividers)
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
                    return ($"Expected divider '{divider}' in value '{raw}'.", null);
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
                    return ($"Expected one of dividers [{string.Join(", ", dividers.Select(d => $"'{d}'"))}] in value '{raw}'.", null);
                parts[i] = remaining[..bestIdx];
                remaining = remaining[(bestIdx + bestDivider.Length)..];
            }
        }
        parts[typeArgs.Length - 1] = remaining;

        var values = new object[typeArgs.Length];
        for (var i = 0; i < typeArgs.Length; i++)
        {
            var (convErr, convResult) = ConvertValue(parts[i], typeArgs[i]);
            if (convErr is not null) return (convErr, null);
            values[i] = convResult!;
        }

        return (null, Activator.CreateInstance(tupleType, values)!);
    }

    private static (string? error, object? result) ConvertValue(string raw, Type targetType)
    {
        // Strip surrounding quotes
        if (raw is ['"', _, ..] && raw[^1] == '"')
            raw = raw[1..^1];

        try
        {
            var result = targetType switch
            {
                _ when targetType == typeof(string)               => raw,
                _ when targetType == typeof(bool)                 => bool.Parse(raw),
                _ when targetType == typeof(int)                  => int.Parse(raw),
                _ when targetType == typeof(DateTime)             => DateTime.Parse(raw),
                _ when targetType == typeof(DateOnly)             => DateOnly.Parse(raw),
                _ when targetType == typeof(TimeOnly)             => TimeOnly.Parse(raw),
                _ when targetType == typeof(TimeSpan)             => TimeSpan.Parse(raw),
                _ when targetType == typeof(Guid)                 => Guid.Parse(raw),
                _ when targetType == typeof(Uri)                  => new Uri(raw),
                _ when targetType == typeof(FileInfo)             => new FileInfo(raw),
                _ when targetType == typeof(DirectoryInfo)        => new DirectoryInfo(raw),
                _ when targetType == typeof(System.Net.IPAddress) => System.Net.IPAddress.Parse(raw),
                _ when targetType == typeof(Version)              => Version.Parse(raw),
                _ when targetType.IsEnum                          => Enum.Parse(targetType, raw, ignoreCase: true),
                _                                                 => Convert.ChangeType(raw, targetType)
            };
            return (null, result);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException or UriFormatException)
        {
            return ($"Invalid value '{raw}' for type '{targetType.Name}'.", null);
        }
    }

    internal static bool IsCollectionProperty(Type type, out Type elementType)
    {
        var underlying = UnwrapNullable(type);

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

    private static (string? error, object? result) MaterializeCollection(Type propType, List<object> items)
    {
        var underlying = UnwrapNullable(propType);

        // Array: T[]
        if (underlying.IsArray)
        {
            var elementType = underlying.GetElementType()!;
            var array = Array.CreateInstance(elementType, items.Count);
            for (var i = 0; i < items.Count; i++)
                array.SetValue(items[i], i);
            return (null, array);
        }

        if (!underlying.IsGenericType)
            return ($"Cannot materialize collection for type '{propType.Name}'.", null);

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

    private static string? ValidateValueConstraints(PropertyRule rule, string value)
    {
        if (rule.IsExistingOnlyFile)
        {
            if (!File.Exists(value))
                return $"File '{value}' does not exist (property '{rule.Property.Name}').";
        }
        if (rule.IsExistingOnlyDirectory)
        {
            if (!Directory.Exists(value))
                return $"Directory '{value}' does not exist (property '{rule.Property.Name}').";
        }
        if (rule.IsLegalFileNamesOnly)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            if (value.IndexOfAny(invalidChars) >= 0)
                return $"Value '{value}' contains illegal file name characters (property '{rule.Property.Name}').";
        }
        if (rule.AcceptFromAmong is not null)
        {
            if (!rule.AcceptFromAmong.Contains(value, StringComparer.OrdinalIgnoreCase))
                return $"Value '{value}' is not accepted for property '{rule.Property.Name}'. Accepted values: {string.Join(", ", rule.AcceptFromAmong)}.";
        }
        return null;
    }

    private static (string? error, object? result) FillCollection(Type collectionType, Type elementType, List<object> items)
    {
        var instance = Activator.CreateInstance(collectionType)!;
        var addMethod = typeof(ICollection<>).MakeGenericType(elementType).GetMethod("Add")
            ?? collectionType.GetMethod("Add", [elementType])
            ?? collectionType.GetMethod("Enqueue", [elementType]);
        if (addMethod is null)
            return ($"Cannot find Add/Enqueue method on '{collectionType.Name}'.", null);
        foreach (var item in items)
            addMethod.Invoke(instance, [item]);
        return (null, instance);
    }

    /// <summary>
    /// Loads key=value pairs from a <c>.env</c> file in the current working directory.
    /// Lines starting with <c>#</c> and blank lines are ignored.
    /// Values may optionally be wrapped in single or double quotes.
    /// </summary>
    internal static Dictionary<string, string> LoadDotEnv(string? path = null)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var filePath = path ?? Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (!File.Exists(filePath))
            return result;

        foreach (var rawLine in File.ReadLines(filePath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            var eq = line.IndexOf('=');
            if (eq <= 0)
                continue;

            var key = line[..eq].Trim();
            var val = line[(eq + 1)..].Trim();

            // Strip surrounding quotes
            if (val.Length >= 2 && ((val[0] == '"' && val[^1] == '"') || (val[0] == '\'' && val[^1] == '\'')))
                val = val[1..^1];

            result[key] = val;
        }
        return result;
    }
}
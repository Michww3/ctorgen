using Ctorgen.Models;
using System.Text;

namespace Ctorgen;

/// <summary>
/// Generates overloaded constructors with a "ladder" initialization chain.
/// Each constructor delegates to the next one, gradually replacing Params 
/// with their hash representations.
/// </summary>
public static class CtorGenerator
{
    // === Naming conventions ===
    private const string HashInterfaceType = "IDeterminedHash";
    private const string FieldPrefix = "_";
    private const string HashSuffix = "Hash";

    // === Formatting constants ===
    private const string Indent1 = "    ";
    private const string Indent2 = "        ";
    private const string LineBreak = "\n";
    private const string CommaLineBreak = "," + LineBreak;

    /// <summary>
    /// Generates overloaded constructor code for the specified class.
    /// </summary>
    /// <param name="className">The name of the class to generate constructors for.</param>
    /// <param name="params">Array of model Params.</param>
    /// <returns>C# source code string containing the generated constructors.</returns>
    public static string Generate(string className, Param[] @params)
    {
        ArgumentNullException.ThrowIfNull(@params);

        ArgumentNullException.ThrowIfNull(className);

        StringBuilder sb = new StringBuilder();
        int paramCount = @params.Length;
        int totalMasks = 1 << paramCount;

        // Order masks: first by number of hash Params, then by numeric value
        IOrderedEnumerable<int> orderedMasks = Enumerable.Range(0, totalMasks)
            .OrderBy(CountBits)
            .ThenBy(mask => mask);

        foreach (int mask in orderedMasks)
        {
            // Skip the final mask — it is handled separately
            if (mask == totalMasks - 1)
            {
                continue;
            }

            int nextMask = BuildNextMask(mask, paramCount);
            _ = sb.AppendLine(BuildDelegatingCtor(className, @params, mask, nextMask));
            _ = sb.AppendLine();
        }

        // Generate the final constructor (all Params as hashes)
        _ = sb.AppendLine(BuildFinalCtor(className, @params));

        return sb.ToString();
    }

    /// <summary>
    /// Builds a constructor that delegates to the next one in the chain.
    /// </summary>
    private static string BuildDelegatingCtor(string className, Param[] @params, int mask, int nextMask)
    {
        string parameterList = BuildParameterList(@params, mask);
        string argumentList = BuildChainedArgumentList(@params, mask, nextMask);

        return $"public {className}({LineBreak}" +
                $"{parameterList}{LineBreak}" +
                $"){LineBreak}" +
                $"{Indent1}: this({LineBreak}" +
                $"{argumentList}{LineBreak}" +
                $"{Indent1}){LineBreak}" +
                $"{Indent1}{{ }}";
    }

    /// <summary>
    /// Builds the constructor parameter list based on the current mask.
    /// </summary>
    private static string BuildParameterList(Param[] @params, int mask)
    {
        IEnumerable<string> paramDeclarations = @params.Select((param, index) =>
        {
            bool useHash = IsHashed(mask, index);
            string type = useHash ? HashInterfaceType : param.Type;
            string name = useHash ? BuildHashParameterName(param.Name) : param.Name;
            return $"{Indent1}{type} {name}";
        });

        return string.Join(CommaLineBreak, paramDeclarations);
    }

    /// <summary>
    /// Builds the argument list for the chained this(...) call.
    /// </summary>
    private static string BuildChainedArgumentList(Param[] @params, int currentMask, int nextMask)
    {
        IEnumerable<string> arguments = @params.Select((param, index) =>
        {
            string value = ResolveArgumentValue(param, index, currentMask, nextMask);
            return $"{Indent2}{value}";
        });

        return string.Join(CommaLineBreak, arguments);
    }

    /// <summary>
    /// Determines which value to pass in the constructor chain.
    /// </summary>
    private static string ResolveArgumentValue(Param param, int index, int currentMask, int nextMask)
    {
        // If parameter is already a hash in current mask — pass the hash parameter name
        if (IsHashed(currentMask, index))
        {
            return BuildHashParameterName(param.Name);
        }

        // If parameter becomes a hash in the next mask — use the hash expression
        if (IsHashed(nextMask, index))
        {
            return param.Hash;
        }

        // Otherwise pass the regular parameter name
        return param.Name;
    }

    /// <summary>
    /// Builds the final constructor that initializes fields directly.
    /// </summary>
    private static string BuildFinalCtor(string className, Param[] @params)
    {
        string parameterList = BuildFinalParameterList(@params);
        string fieldAssignments = BuildFieldAssignments(@params);

        return $"public {className}({LineBreak}" +
                $"{parameterList}{LineBreak}" +
                $"){LineBreak}" +
                $"{Indent1}{{{LineBreak}" +
                $"{fieldAssignments}{LineBreak}" +
                $"{Indent1}}}";
    }

    /// <summary>
    /// Builds the parameter list for the final constructor (all Params as hashes).
    /// </summary>
    private static string BuildFinalParameterList(Param[] @params)
    {
        IEnumerable<string> paramDeclarations = @params.Select(param =>
            $"{Indent1}{HashInterfaceType} {BuildHashParameterName(param.Name)}");

        return string.Join(CommaLineBreak, paramDeclarations);
    }

    /// <summary>
    /// Builds field assignment statements for the final constructor body.
    /// </summary>
    private static string BuildFieldAssignments(Param[] @params)
    {
        IEnumerable<string> assignments = @params.Select(param =>
            $"{Indent1}{BuildFieldName(param.Name)} = {BuildHashParameterName(param.Name)};");

        return string.Join(LineBreak, assignments);
    }

    /// <summary>
    /// Computes the next mask in the "ladder" sequence.
    /// Finds the first non-hashed parameter and adds it to hash mode.
    /// </summary>
    private static int BuildNextMask(int currentMask, int paramCount)
    {
        int bitToSet = FindFirstUnhashedBit(currentMask, paramCount);
        return bitToSet == -1 ? currentMask : currentMask | (1 << bitToSet);
    }

    /// <summary>
    /// Finds the index of the first parameter that is not yet represented as a hash.
    /// </summary>
    private static int FindFirstUnhashedBit(int mask, int paramCount)
    {
        for (int i = 0; i < paramCount; i++)
        {
            if (!IsHashed(mask, i))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Checks whether a parameter is represented as a hash in the given mask.
    /// </summary>
    private static bool IsHashed(int mask, int parameterIndex)
    {
        return (mask & (1 << parameterIndex)) != 0;
    }

    /// <summary>
    /// Formats a hash parameter name: {originalName}Hash.
    /// </summary>
    private static string BuildHashParameterName(string originalName)
    {
        return originalName + HashSuffix;
    }

    /// <summary>
    /// Formats a field name for storing a hash: _{originalName}Hash.
    /// </summary>
    private static string BuildFieldName(string originalName)
    {
        return FieldPrefix + originalName + HashSuffix;
    }

    /// <summary>
    /// Counts the number of set bits in an integer.
    /// Used for sorting masks by complexity.
    /// </summary>
    private static int CountBits(int value)
    {
        int count = 0;
        while (value != 0)
        {
            count += value & 1;
            value >>= 1;
        }

        return count;
    }
}

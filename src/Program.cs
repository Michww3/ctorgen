using Ctorgen.Models;
using System.Text.Json;

namespace Ctorgen;

/// <summary>
/// Entry point for the CtorGenerator .NET tool.
/// Generates overloaded constructors with a "ladder" initialization chain.
/// </summary>
public static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static int Main(string[] args)
    {
        try
        {
            if (HandleInit(args))
            {
                return 0;
            }

            InputModel input = ParseArgs(args);
            input = LoadFromJson(input);
            Validate(input);
            Normalize(input);

            string result = Generate(input);
            WriteOutput(result, input);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine();
            PrintUsage();
            return 1;
        }
    }

    private static bool HandleInit(string[] args)
    {
        if (args.Length > 0 && args[0] == "init")
        {
            string path = args.Length > 1 ? args[1] : "ctorgen.json";
            CreateTemplate(path);
            Console.WriteLine($"Config template created: {path}");
            return true;
        }

        if (args.Any(a => a is "--help" or "-h" or "/?") || args.Length == 0)
        {
            PrintUsage();
            return true;
        }

        return false;
    }

    private static InputModel ParseArgs(string[] args)
    {
        InputModel input = new InputModel();

        foreach (string arg in args)
        {
            if (arg.StartsWith("--config=", StringComparison.Ordinal))
            {
                input.ConfigPath = arg["--config=".Length..];
            }
            else if (arg.StartsWith("--name=", StringComparison.Ordinal))
            {
                input.ClassName = arg["--name=".Length..];
            }
            else if (arg.StartsWith("--out=", StringComparison.Ordinal))
            {
                input.OutputPath = arg["--out=".Length..];
            }
            else if (arg.StartsWith("--param=", StringComparison.Ordinal))
            {
                input.Params.Add(ParseParam(arg));
            }
            else
            {
                throw new ArgumentException($"Unknown argument: {arg}");
            }
        }

        return input;
    }

    private static Param ParseParam(string arg)
    {
        // Format: --param=name:type:hash
        string value = arg["--param=".Length..];
        string[] parts = value.Split(':');

        return parts.Length != 3
            ? throw new ArgumentException($"Invalid param format: '{arg}'. Expected: --param=name:type:hash")
            : new Param(
            name: parts[0],
            type: parts[1],
            hash: parts[2]
        );
    }

    private static InputModel LoadFromJson(InputModel input)
    {
        if (string.IsNullOrEmpty(input.ConfigPath))
        {
            return input;
        }

        if (!File.Exists(input.ConfigPath))
        {
            throw new FileNotFoundException($"Config file not found: {input.ConfigPath}");
        }

        string json = File.ReadAllText(input.ConfigPath);
        GeneratorConfig config = JsonSerializer.Deserialize<GeneratorConfig>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize config: empty or invalid JSON");

        // CLI args override config file values
        return new InputModel
        {
            ConfigPath = input.ConfigPath,
            ClassName = input.ClassName ?? config.ClassName,
            Params = input.Params.Count > 0 ? input.Params : [.. config.Params],
            OutputPath = input.OutputPath ?? $"{input.ClassName ?? config.ClassName}.Ctors.cs"
        };
    }

    private static void Validate(InputModel input)
    {
        if (string.IsNullOrEmpty(input.ClassName))
        {
            throw new ArgumentException("--name or config with ClassName is required");
        }

        if (input.Params.Count == 0)
        {
            throw new ArgumentException("No Params provided. Use --param=... or config file");
        }

        // Validate each parameter
        foreach (Param param in input.Params)
        {
            if (string.IsNullOrEmpty(param.Name))
            {
                throw new ArgumentException("Parameter name cannot be empty");
            }

            if (string.IsNullOrEmpty(param.Type))
            {
                throw new ArgumentException($"Parameter '{param.Name}' type cannot be empty");
            }

            if (string.IsNullOrEmpty(param.Hash))
            {
                throw new ArgumentException($"Parameter '{param.Name}' hash expression cannot be empty");
            }
        }
    }

    private static void Normalize(InputModel input)
    {
        input.OutputPath ??= $"{input.ClassName}.Ctors.cs";
    }

    private static string Generate(InputModel input)
    {
        string code = CtorGenerator.Generate(input.ClassName!, [.. input.Params]);

        return code;
    }

    private static void WriteOutput(string result, InputModel input)
    {
        // Ensure directory exists
        string? directory = Path.GetDirectoryName(input.OutputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }

        File.WriteAllText(input.OutputPath!, result);
        Console.WriteLine($"Constructors generated: {input.OutputPath}");
    }

    private static void CreateTemplate(string path)
    {
        GeneratorConfig config = new GeneratorConfig
        (
            className: "MyModelHash",
            @params:
            [
                new Param(
                    name: "id",
                    type: "IGuid",
                    hash: "new DeterminedHash(id)"),
                new Param(
                    name: "name",
                    type: "IString",
                    hash: "new DeterminedHash(name)"),
                new Param(
                    name: "timestamp",
                    type: "DateTime",
                    hash: "new DeterminedHash(timestamp)")
            ]
        );

        string json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(path, json);
    }

    private static void PrintUsage()
    {
        Console.WriteLine("CtorGenerator — .NET tool for generating ladder-style constructors");
        Console.WriteLine();
        Console.WriteLine("Usage: ctorgen [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  init [path]           Create a config template (default: ctorgen.json)");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --config=PATH         Path to JSON config file");
        Console.WriteLine("  --name=CLASS          Target class name (required if not in config)");
        Console.WriteLine("  --out=FILE            Output file path (default: {ClassName}.Ctors.cs)");
        Console.WriteLine("  --param=DEF           Parameter definition:");
        Console.WriteLine("                        name:type:hashExpr");
        Console.WriteLine("                        Example: --param=id:IGuid:new DeterminedHash(id)");
        Console.WriteLine("                        (Can be specified multiple times)");
        Console.WriteLine("  --help, -h, /?        Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  ctorgen --name=MyHash --param=id:IGuid:new DeterminedHash(id)");
        Console.WriteLine("  ctorgen --config=myconfig.json --out=Generated/MyHash.cs");
        Console.WriteLine("  ctorgen init mytool.json");
    }
}

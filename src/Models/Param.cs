namespace CtorGenerator.Models;

public sealed record Param
{
    public string Type { get; init; }
    public string Name { get; init; }
    public string Hash { get; init; }

    public Param(string name, string type, string hash)
    {
        Type = type;
        Name = name;
        Hash = hash;
    }
}
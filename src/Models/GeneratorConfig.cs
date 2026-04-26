namespace Ctorgen.Models;

public sealed record GeneratorConfig
{
    public string ClassName { get; init; }
    public Param[] Params { get; init; }

    public GeneratorConfig(string className, Param[] @params)
    {
        ClassName = className;
        Params = @params;
    }
}

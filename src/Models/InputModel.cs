namespace Ctorgen.Models;

public class InputModel
{
    public string? ConfigPath { get; set; }
    public string? ClassName { get; set; }
    public string? OutputPath { get; set; }
    public List<Param> Params { get; set; } = [];

}

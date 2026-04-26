namespace CtorGenerator;

public static class Programm
{
    static void Main()
    {
        string className = "AxisRichRelationalModelHash";

        var parameters = new[]
        {
            new Param("IGuid","id","new DeterminedHash(id)"),
            new Param("IGuid","chartId","new DeterminedHash(chartId)"),
            new Param("IString","legend","new DeterminedHash(legend)"),
            new Param("IString","legend2","new DeterminedHash(legend2)"),
            new Param("IString","legend3","new DeterminedHash(legend3)"),
        };

        string generatedCode = CtorGenerator.Generate(className, parameters);
        Console.WriteLine(generatedCode);
    }
}

namespace CtorGenerator;

public class Param
{
    public string Type;
    public string Name;
    public string HashExpr;

    public Param(string t, string n, string h)
    {
        Type = t;
        Name = n;
        HashExpr = h;
    }
}
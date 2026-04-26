namespace CtorGenerator;

public static class CtorGenerator
{
    public static string Generate(string className, Param[] p)
    {
        var sb = new System.Text.StringBuilder();

        int n = p.Length;
        int total = 1 << n;

        const string I1 = "    ";
        const string I2 = "        ";

        // 1. build all masks
        int[] masks = new int[total];
        for (int i = 0; i < total; i++)
            masks[i] = i;

        // 2. SORT ONLY FOR OUTPUT ORDER (NOT LOGIC)
        Array.Sort(masks, (a, b) =>
        {
            int cmp = CountBits(a).CompareTo(CountBits(b));
            return cmp != 0 ? cmp : a.CompareTo(b);
        });

        foreach (var mask in masks)
        {
            if (mask == total - 1)
                continue;

            int next = BuildNextMask(mask, n);

            WriteCtor(sb, className, p, mask, next, n, I1, I2);
        }

        return sb.ToString();
    }

    // =========================
    // LADDER TRANSITION (LOGIC)
    // =========================
    static int BuildNextMask(int mask, int n)
    {
        int bit = GetNextToConvert(mask, n);
        return bit == -1 ? mask : mask | (1 << bit);
    }

    // first non-hash (pure ladder rule)
    static int GetNextToConvert(int mask, int n)
    {
        for (int i = 0; i < n; i++)
        {
            if ((mask & (1 << i)) == 0)
                return i;
        }

        return -1;
    }

    // =========================
    // CODE RENDERING ONLY
    // =========================
    static void WriteCtor(
        System.Text.StringBuilder sb,
        string className,
        Param[] p,
        int mask,
        int next,
        int n,
        string I1,
        string I2)
    {
        sb.AppendLine($"public {className}(");

        // signature
        for (int i = 0; i < n; i++)
        {
            bool isHash = (mask & (1 << i)) != 0;

            string type = isHash ? "IDeterminedHash" : p[i].Type;
            string name = isHash ? p[i].Name + "Hash" : p[i].Name;

            string comma = i < n - 1 ? "," : "";

            sb.AppendLine($"{I1}{type} {name}{comma}");
        }

        sb.AppendLine(")");
        sb.AppendLine($"{I1}: this(");

        // arguments
        for (int i = 0; i < n; i++)
        {
            bool wasHash = (mask & (1 << i)) != 0;
            bool becomesHash = (next & (1 << i)) != 0;

            string value;

            if (wasHash)
            {
                value = p[i].Name + "Hash";
            }
            else if (becomesHash)
            {
                value = p[i].HashExpr;
            }
            else
            {
                value = p[i].Name;
            }

            string comma = i < n - 1 ? "," : "";

            sb.AppendLine($"{I2}{value}{comma}");
        }

        sb.AppendLine(")");
        sb.AppendLine("{ }");
        sb.AppendLine();
    }

    // =========================
    // UTIL
    // =========================
    static int CountBits(int x)
    {
        int count = 0;

        while (x != 0)
        {
            count += x & 1;
            x >>= 1;
        }

        return count;
    }
}
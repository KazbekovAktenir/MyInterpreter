using System.Text;

namespace MyInterpreter.Core;

public class Quadruple
{
    public string Op { get; set; }
    public string Arg1 { get; set; }
    public string Arg2 { get; set; }
    public string Result { get; set; }

    public Quadruple(string op, string arg1, string arg2, string result)
    {
        Op = op;
        Arg1 = arg1;
        Arg2 = arg2;
        Result = result;
    }

    public override string ToString() =>
        $"({EscapeField(Op)}, {EscapeField(Arg1)}, {EscapeField(Arg2)}, {EscapeField(Result)})";

    public static Quadruple ParseLine(string line)
    {
        string inner = line.Trim();
        if (inner.Length < 2 || inner[0] != '(' || inner[^1] != ')')
            throw new InvalidOperationException($"Неверный формат строки четвёрки: {line}");
        inner = inner[1..^1];
        string[] parts = SplitFields(inner, 4);
        return new Quadruple(parts[0], parts[1], parts[2], parts[3]);
    }

    private static string[] SplitFields(string inner, int count)
    {
        var parts = new string[count];
        int field = 0;
        int i = 0;
        while (field < count && i <= inner.Length)
        {
            int end = field == count - 1 ? inner.Length : NextCommaTopLevel(inner, i);
            parts[field++] = UnescapeField(inner[i..end].Trim());
            i = end + 1;
        }

        if (field != count)
            throw new InvalidOperationException($"Ожидалось {count} полей в четвёрке: {inner}");
        return parts;
    }

    private static int NextCommaTopLevel(string s, int start)
    {
        bool inString = false;
        for (int i = start; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '"' && (i == start || s[i - 1] != '\\'))
                inString = !inString;
            else if (c == ',' && !inString)
                return i;
        }

        return s.Length;
    }

    private static string EscapeField(string s)
    {
        if (s.IndexOfAny([',', '"', '\n', '\r']) < 0)
            return s;
        return '"' + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + '"';
    }

    private static string UnescapeField(string s)
    {
        s = s.Trim();
        if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
        {
            s = s[1..^1];
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    i++;
                    sb.Append(s[i] switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        '"' => '"',
                        '\\' => '\\',
                        _ => s[i]
                    });
                }
                else
                    sb.Append(s[i]);
            }

            return sb.ToString();
        }

        return s;
    }
}

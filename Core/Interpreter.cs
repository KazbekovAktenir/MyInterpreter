using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MyInterpreter.Core;

public class Interpreter
{
    private readonly List<Quadruple> _quads;
    private readonly SymbolTable _symbols = new();

    public Interpreter(List<Quadruple> quads) => _quads = quads;

    public string Run()
    {
        var output = new StringBuilder();
        int pc = 0;
        while (pc >= 0 && pc < _quads.Count)
        {
            var q = _quads[pc];
            int next = pc + 1;

            switch (q.Op)
            {
                case "=":
                    _symbols.Set(q.Result, GetValue(q.Arg1));
                    break;
                case "+":
                    _symbols.Set(q.Result, GetValue(q.Arg1) + GetValue(q.Arg2));
                    break;
                case "-":
                    _symbols.Set(q.Result, GetValue(q.Arg1) - GetValue(q.Arg2));
                    break;
                case "*":
                    _symbols.Set(q.Result, GetValue(q.Arg1) * GetValue(q.Arg2));
                    break;
                case "/":
                    _symbols.Set(q.Result, GetValue(q.Arg1) / GetValue(q.Arg2));
                    break;
                case "<=":
                    _symbols.Set(q.Result, GetValue(q.Arg1) <= GetValue(q.Arg2) ? 1 : 0);
                    break;
                case "JZ":
                    if (GetValue(q.Arg1) == 0)
                        next = int.Parse(q.Result, CultureInfo.InvariantCulture);
                    break;
                case "JMP":
                    next = int.Parse(q.Arg1, CultureInfo.InvariantCulture);
                    break;
                case "WRITE":
                    output.AppendLine(GetValue(q.Arg1).ToString(CultureInfo.InvariantCulture));
                    break;
                case "WRITELIT":
                    output.AppendLine(q.Arg1);
                    break;
                default:
                    throw new InvalidOperationException($"Неизвестная операция четвёрки: '{q.Op}'.");
            }

            pc = next;
        }

        return output.ToString();
    }

    private int GetValue(string nameOrLiteral)
    {
        if (int.TryParse(nameOrLiteral, NumberStyles.Integer, CultureInfo.InvariantCulture, out int lit))
            return lit;
        return _symbols.Get(nameOrLiteral);
    }

    public void SaveResult(string path) => File.WriteAllText(path, Run(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

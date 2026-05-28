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

    public Interpreter() : this(new List<Quadruple>())
    {
    }

    public Interpreter(List<Quadruple> quads) => _quads = quads;

    public string Run() => Execute(_quads);

    public string Execute(List<Quadruple> quads)
    {
        var output = new StringBuilder();
        int pc = 0;
        while (pc >= 0 && pc < quads.Count)
        {
            var q = quads[pc];
            int next = pc + 1;

            switch (q.Op)
            {
                case "=":
                    _symbols.Set(q.Result, GetValue(q.Arg1));
                    break;
                case "SETSTR":
                    _symbols.Set(q.Result, RuntimeValue.FromString(q.Arg1));
                    break;
                case "+":
                    _symbols.Set(q.Result, AddValues(GetValue(q.Arg1), GetValue(q.Arg2)));
                    break;
                case "-":
                    _symbols.Set(q.Result, RuntimeValue.FromNumber(GetValue(q.Arg1).AsNumber() - GetValue(q.Arg2).AsNumber()));
                    break;
                case "*":
                    _symbols.Set(q.Result, RuntimeValue.FromNumber(GetValue(q.Arg1).AsNumber() * GetValue(q.Arg2).AsNumber()));
                    break;
                case "/":
                    double divisor = GetValue(q.Arg2).AsNumber();
                    if (divisor == 0)
                        throw new DivideByZeroException("Деление на ноль.");
                    _symbols.Set(q.Result, RuntimeValue.FromNumber(GetValue(q.Arg1).AsNumber() / divisor));
                    break;
                case "<=":
                    _symbols.Set(q.Result, RuntimeValue.FromInt(GetValue(q.Arg1).AsNumber() <= GetValue(q.Arg2).AsNumber() ? 1 : 0));
                    break;
                case "JZ":
                    if (GetValue(q.Arg1).AsNumber() == 0)
                        next = int.Parse(q.Result, CultureInfo.InvariantCulture);
                    break;
                case "JMP":
                    next = int.Parse(q.Arg1, CultureInfo.InvariantCulture);
                    break;
                case "WRITE":
                    output.AppendLine(GetValue(q.Arg1).ToDisplayString());
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

    private RuntimeValue GetValue(string nameOrLiteral)
    {
        if (double.TryParse(nameOrLiteral, NumberStyles.Float, CultureInfo.InvariantCulture, out double lit))
            return RuntimeValue.FromNumber(lit);
        return _symbols.Get(nameOrLiteral);
    }

    private static RuntimeValue AddValues(RuntimeValue left, RuntimeValue right)
    {
        if (left.Type == RuntimeValueType.String || right.Type == RuntimeValueType.String)
            return RuntimeValue.FromString(left.ToDisplayString() + right.ToDisplayString());
        return RuntimeValue.FromNumber(left.AsNumber() + right.AsNumber());
    }

    public void SaveResult(string path) => File.WriteAllText(path, Run(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

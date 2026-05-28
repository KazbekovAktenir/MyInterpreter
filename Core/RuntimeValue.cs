using System;
using System.Globalization;

namespace MyInterpreter.Core;

public readonly struct RuntimeValue
{
    private readonly double _numberValue;
    private readonly string? _stringValue;

    private RuntimeValue(double value)
    {
        _numberValue = value;
        _stringValue = null;
        Type = RuntimeValueType.Number;
    }

    private RuntimeValue(string value)
    {
        _numberValue = 0;
        _stringValue = value;
        Type = RuntimeValueType.String;
    }

    public RuntimeValueType Type { get; }

    public static RuntimeValue FromInt(int value) => new(value);

    public static RuntimeValue FromNumber(double value) => new(value);

    public static RuntimeValue FromString(string value) => new(value);

    public double AsNumber()
    {
        if (Type == RuntimeValueType.Number)
            return _numberValue;
        throw new InvalidOperationException("Операция ожидает число, но получена строка.");
    }

    public string ToDisplayString() =>
        Type switch
        {
            RuntimeValueType.Number => _numberValue.ToString("0.################", CultureInfo.InvariantCulture),
            RuntimeValueType.String => _stringValue ?? "",
            _ => ""
        };
}

public enum RuntimeValueType
{
    Number,
    String
}

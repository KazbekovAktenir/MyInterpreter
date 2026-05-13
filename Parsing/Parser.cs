using System;
using System.Collections.Generic;
using MyInterpreter.Core;

namespace MyInterpreter.Parsing;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _pos;
    private int _tempCount;

    public List<Quadruple> Quads { get; } = new();

    public Parser(List<Token> tokens) => _tokens = tokens;

    private Token Current => _pos < _tokens.Count ? _tokens[_pos] : _tokens[^1];

    private Token Match(TokenType type)
    {
        if (Current.Type != type)
            throw new InvalidOperationException($"Ожидался {type}, получен {Current.Type} ('{Current.Value}').");
        return _tokens[_pos++];
    }

    private string NewTemp() => $"T{_tempCount++}";

    public void Parse()
    {
        while (Current.Type != TokenType.EOF)
            ParseStatement();
    }

    private void ParseStatement()
    {
        switch (Current.Type)
        {
            case TokenType.Identifier:
                ParseAssignment();
                break;
            case TokenType.For:
                ParseForLoop();
                break;
            case TokenType.Write:
                ParseWrite();
                break;
            case TokenType.EOF:
                return;
            default:
                throw new InvalidOperationException($"Неожиданная лексема: {Current.Type} ('{Current.Value}').");
        }
    }

    private void ParseAssignment()
    {
        string target = Match(TokenType.Identifier).Value;
        Match(TokenType.Assign);
        string exprResult = ParseExpression();
        Quads.Add(new Quadruple("=", exprResult, "_", target));
    }

    private string ParseExpression() => ParseAddSub();

    private string ParseAddSub()
    {
        string left = ParseMulDiv();
        while (Current.Type is TokenType.Plus or TokenType.Minus)
        {
            string op = Match(Current.Type).Value;
            string right = ParseMulDiv();
            string temp = NewTemp();
            Quads.Add(new Quadruple(op, left, right, temp));
            left = temp;
        }

        return left;
    }

    private string ParseMulDiv()
    {
        string left = ParseFactor();
        while (Current.Type is TokenType.Multiply or TokenType.Divide)
        {
            string op = Match(Current.Type).Value;
            string right = ParseFactor();
            string temp = NewTemp();
            Quads.Add(new Quadruple(op, left, right, temp));
            left = temp;
        }

        return left;
    }

    private string ParseFactor()
    {
        switch (Current.Type)
        {
            case TokenType.Number:
                return Match(TokenType.Number).Value;
            case TokenType.Identifier:
                return Match(TokenType.Identifier).Value;
            case TokenType.LeftParen:
                Match(TokenType.LeftParen);
                string inner = ParseExpression();
                Match(TokenType.RightParen);
                return inner;
            default:
                throw new InvalidOperationException($"Ожидалось выражение, получено {Current.Type}.");
        }
    }

    private void ParseForLoop()
    {
        Match(TokenType.For);
        string iter = Match(TokenType.Identifier).Value;
        Match(TokenType.Assign);
        string startVal = ParseExpression();
        Quads.Add(new Quadruple("=", startVal, "_", iter));

        Match(TokenType.To);
        string endVal = ParseExpression();

        int loopHead = Quads.Count;
        string tCond = NewTemp();
        Quads.Add(new Quadruple("<=", iter, endVal, tCond));
        int jzIndex = Quads.Count;
        Quads.Add(new Quadruple("JZ", tCond, "_", "0"));

        Match(TokenType.Do);
        Match(TokenType.Begin);
        while (Current.Type != TokenType.End)
            ParseStatement();
        Match(TokenType.End);

        Quads.Add(new Quadruple("+", iter, "1", iter));
        Quads.Add(new Quadruple("JMP", loopHead.ToString(), "_", "_"));

        int exitPc = Quads.Count;
        Quads[jzIndex] = new Quadruple("JZ", tCond, "_", exitPc.ToString());
    }

    private void ParseWrite()
    {
        Match(TokenType.Write);
        switch (Current.Type)
        {
            case TokenType.StringLiteral:
                Quads.Add(new Quadruple("WRITELIT", Match(TokenType.StringLiteral).Value, "_", "_"));
                break;
            case TokenType.Identifier:
                Quads.Add(new Quadruple("WRITE", Match(TokenType.Identifier).Value, "_", "_"));
                break;
            case TokenType.Number:
                Quads.Add(new Quadruple("WRITE", Match(TokenType.Number).Value, "_", "_"));
                break;
            default:
                throw new InvalidOperationException("После ВЫВЕСТИ ожидается строка в кавычках, число или имя переменной.");
        }
    }
}

using System;
using System.Collections.Generic;
using MyInterpreter.Core;

namespace MyInterpreter.Lexing;

public class Lexer
{
    private readonly string _source;
    private int _pos;

    private readonly Dictionary<string, TokenType> _keywords = new()
    {
        { "ДЛЯ", TokenType.For },
        { "ДО", TokenType.To },
        { "ВЫПОЛНЯТЬ", TokenType.Do },
        { "НАЧАЛО", TokenType.Begin },
        { "КОНЕЦ", TokenType.End },
        { "ВЫВЕСТИ", TokenType.Write }
    };

    public Lexer(string source) => _source = source;

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (_pos < _source.Length)
        {
            char current = _source[_pos];

            if (char.IsWhiteSpace(current) || current == '\uFEFF')
            {
                _pos++;
                continue;
            }

            if (current == '"')
            {
                tokens.Add(ReadString());
                continue;
            }

            if (char.IsLetter(current) || current == '_')
            {
                string word = "";
                while (_pos < _source.Length && (char.IsLetterOrDigit(_source[_pos]) || _source[_pos] == '_'))
                    word += _source[_pos++];

                if (_keywords.TryGetValue(word, out var kw))
                    tokens.Add(new Token(kw, word));
                else
                    tokens.Add(new Token(TokenType.Identifier, word));
                continue;
            }

            if (char.IsDigit(current))
            {
                string num = "";
                bool hasDot = false;
                while (_pos < _source.Length && (char.IsDigit(_source[_pos]) || _source[_pos] == '.'))
                {
                    if (_source[_pos] == '.')
                    {
                        if (hasDot)
                            throw new InvalidOperationException($"Неверное число на позиции {_pos}.");
                        hasDot = true;
                    }
                    num += _source[_pos++];
                }
                tokens.Add(new Token(TokenType.Number, num));
                continue;
            }

            if (current == ':' && _pos + 1 < _source.Length && _source[_pos + 1] == '=')
            {
                tokens.Add(new Token(TokenType.Assign, ":="));
                _pos += 2;
                continue;
            }

            switch (current)
            {
                case '+':
                    tokens.Add(new Token(TokenType.Plus, "+"));
                    break;
                case '-':
                    tokens.Add(new Token(TokenType.Minus, "-"));
                    break;
                case '*':
                    tokens.Add(new Token(TokenType.Multiply, "*"));
                    break;
                case '/':
                    tokens.Add(new Token(TokenType.Divide, "/"));
                    break;
                case '(':
                    tokens.Add(new Token(TokenType.LeftParen, "("));
                    break;
                case ')':
                    tokens.Add(new Token(TokenType.RightParen, ")"));
                    break;
                default:
                    throw new InvalidOperationException($"Неожиданный символ '{current}' на позиции {_pos}.");
            }

            _pos++;
        }

        tokens.Add(new Token(TokenType.EOF, ""));
        return tokens;
    }

    private Token ReadString()
    {
        _pos++; // "
        var sb = new System.Text.StringBuilder();
        while (_pos < _source.Length)
        {
            char c = _source[_pos++];
            if (c == '"')
                return new Token(TokenType.StringLiteral, sb.ToString());
            if (c == '\\' && _pos < _source.Length)
            {
                char n = _source[_pos++];
                sb.Append(n == '"' ? '"' : n);
            }
            else
                sb.Append(c);
        }

        throw new InvalidOperationException("Незакрытая строка в кавычках.");
    }
}

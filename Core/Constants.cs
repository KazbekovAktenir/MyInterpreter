namespace MyInterpreter.Core
{
    public enum TokenType
    {
        // Ключевые слова
        For, To, Do, Begin, End, Write,
        // Операнды и символы
        Identifier, Number, StringLiteral, Assign,
        Plus, Minus, Multiply, Divide,
        LeftParen, RightParen,
        EOF
    }
}
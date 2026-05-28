using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MyInterpreter.Core;
using MyInterpreter.IO;
using MyInterpreter.Lexing;
using MyInterpreter.Parsing;

string baseDir = AppContext.BaseDirectory;
string inputPath = Path.Combine(baseDir, "input.pc");
string quadsPath = Path.Combine(baseDir, "quads.txt");
string resultPath = Path.Combine(baseDir, "result.txt");

if (!File.Exists(inputPath))
{
    Console.Error.WriteLine($"Файл не найден: {inputPath}");
    Console.Error.WriteLine("Поместите исходный текст в input.pc рядом с исполняемым файлом.");
    return 1;
}

var interpreter = new Interpreter();
var executedQuads = new List<Quadruple>();
var result = new StringBuilder();
var statementLines = new List<string>();
int statementStartLine = 0;
int blockDepth = 0;
bool pendingBlock = false;
bool blockStarted = false;

foreach (var item in File.ReadLines(inputPath).Select((Text, Index) => new { Text, Line = Index + 1 }))
{
    string line = item.Line == 1 ? item.Text.TrimStart('\uFEFF') : item.Text;
    if (statementLines.Count == 0 && string.IsNullOrWhiteSpace(line))
        continue;

    List<Token> lineTokens;
    try
    {
        lineTokens = new Lexer(line).Tokenize();
    }
    catch (Exception ex)
    {
        SavePartialResult(resultPath, result, item.Line, ex.Message);
        FileManager.SaveQuads(quadsPath, executedQuads);
        Console.Error.WriteLine($"Ошибка в строке {item.Line}: {ex.Message}");
        return 1;
    }

    if (statementLines.Count == 0)
        statementStartLine = item.Line;
    statementLines.Add(line);

    foreach (var token in lineTokens)
    {
        if (token.Type == TokenType.For)
            pendingBlock = true;
        else if (token.Type == TokenType.Begin)
        {
            blockStarted = true;
            blockDepth++;
        }
        else if (token.Type == TokenType.End)
        {
            blockDepth--;
            if (blockDepth < 0)
            {
                string message = "КОНЕЦ без соответствующего НАЧАЛО.";
                SavePartialResult(resultPath, result, item.Line, message);
                FileManager.SaveQuads(quadsPath, executedQuads);
                Console.Error.WriteLine($"Ошибка в строке {item.Line}: {message}");
                return 1;
            }
        }
    }

    if (pendingBlock && (!blockStarted || blockDepth > 0))
        continue;

    if (!ExecuteStatement(string.Join(Environment.NewLine, statementLines), statementStartLine))
        return 1;

    statementLines.Clear();
    blockDepth = 0;
    pendingBlock = false;
    blockStarted = false;
}

if (statementLines.Count > 0)
{
    string message = pendingBlock ? "Незавершенный блок: ожидался КОНЕЦ." : "Незавершенная инструкция.";
    SavePartialResult(resultPath, result, statementStartLine, message);
    FileManager.SaveQuads(quadsPath, executedQuads);
    Console.Error.WriteLine($"Ошибка в строке {statementStartLine}: {message}");
    return 1;
}

FileManager.SaveQuads(quadsPath, executedQuads);
File.WriteAllText(resultPath, result.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

Console.WriteLine($"Сохранено: {quadsPath}");
Console.WriteLine($"Результат: {resultPath}");
return 0;

bool ExecuteStatement(string statementSource, int line)
{
    try
    {
        var lexer = new Lexer(statementSource);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        parser.Parse();

        executedQuads.AddRange(parser.Quads);
        result.Append(interpreter.Execute(parser.Quads));
        return true;
    }
    catch (Exception ex)
    {
        SavePartialResult(resultPath, result, line, ex.Message);
        FileManager.SaveQuads(quadsPath, executedQuads);
        Console.Error.WriteLine($"Ошибка в строке {line}: {ex.Message}");
        return false;
    }
}

static void SavePartialResult(string path, StringBuilder output, int line, string message)
{
    var text = new StringBuilder(output.ToString());
    text.AppendLine($"Ошибка в строке {line}: {message}");
    File.WriteAllText(path, text.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

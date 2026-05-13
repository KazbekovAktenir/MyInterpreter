using System;
using System.IO;
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

string source = FileManager.ReadSource(inputPath);
var lexer = new Lexer(source);
var tokens = lexer.Tokenize();
var parser = new Parser(tokens);
parser.Parse();

FileManager.SaveQuads(quadsPath, parser.Quads);

var quadsFromFile = FileManager.ReadQuads(quadsPath);
var interpreter = new Interpreter(quadsFromFile);
interpreter.SaveResult(resultPath);

Console.WriteLine($"Сохранено: {quadsPath}");
Console.WriteLine($"Результат: {resultPath}");
return 0;

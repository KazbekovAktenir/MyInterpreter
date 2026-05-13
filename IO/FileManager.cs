using System.Collections.Generic;
using System.IO;
using MyInterpreter.Core;

namespace MyInterpreter.IO;

public static class FileManager
{
    public static string ReadSource(string path) => File.ReadAllText(path);

    public static void SaveQuads(string path, List<Quadruple> quads)
    {
        using var sw = new StreamWriter(path);
        foreach (var q in quads)
            sw.WriteLine(q.ToString());
    }

    public static List<Quadruple> ReadQuads(string path)
    {
        var list = new List<Quadruple>();
        foreach (string line in File.ReadAllLines(path))
        {
            string t = line.Trim();
            if (t.Length == 0)
                continue;
            list.Add(Quadruple.ParseLine(t));
        }

        return list;
    }
}

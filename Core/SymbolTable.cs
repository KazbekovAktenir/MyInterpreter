using System.Collections.Generic;

namespace MyInterpreter.Core
{
    public class SymbolTable
    {
        private Dictionary<string, int> _variables = new Dictionary<string, int>();

        public void Set(string name, int value) => _variables[name] = value;

        public int Get(string name)
        {
            if (_variables.ContainsKey(name)) return _variables[name];
            return 0; // По умолчанию 0, если переменная не найдена
        }

        public bool Exists(string name) => _variables.ContainsKey(name);
    }
}
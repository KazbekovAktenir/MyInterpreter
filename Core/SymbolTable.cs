using System;
using System.Collections.Generic;

namespace MyInterpreter.Core
{
    public class SymbolTable
    {
        private Dictionary<string, RuntimeValue> _variables = new Dictionary<string, RuntimeValue>();

        public void Set(string name, RuntimeValue value) => _variables[name] = value;

        public RuntimeValue Get(string name)
        {
            if (_variables.TryGetValue(name, out RuntimeValue value))
                return value;
            throw new InvalidOperationException($"Переменная '{name}' не объявлена.");
        }

        public bool Exists(string name) => _variables.ContainsKey(name);
    }
}

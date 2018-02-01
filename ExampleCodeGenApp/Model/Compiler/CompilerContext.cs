using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleCodeGenApp.Model.Compiler
{
    public class CompilerContext
    {
        public Stack<List<string>> VariablesScopesStack { get; } = new Stack<List<string>>();
        
        public string FindFreeVariableName()
        {
            return "v" + VariablesScopesStack.SelectMany(l => l).Count();
        }

        public void AddVariableToCurrentScope(string variableName)
        {
            VariablesScopesStack.Peek().Add(variableName);
        }

        public void EnterNewScope()
        {
            VariablesScopesStack.Push(new List<string>());
        }

        public void LeaveScope()
        {
            VariablesScopesStack.Pop();
        }
    }
}

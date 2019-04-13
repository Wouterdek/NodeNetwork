using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleCodeGenApp.Model.Compiler
{
    public class ScopeDefinition
    {
        public string Identifier { get; }

        public List<IVariableDefinition> Variables { get; } = new List<IVariableDefinition>();

        public ScopeDefinition(string identifier)
        {
            Identifier = identifier;
        }
    }

    public class CompilerContext
    {
        public Stack<ScopeDefinition> VariablesScopesStack { get; } = new Stack<ScopeDefinition>();
        
        public string FindFreeVariableName()
        {
            return "v" + VariablesScopesStack.SelectMany(s => s.Variables).Count();
        }

        public void AddVariableToCurrentScope(IVariableDefinition variable)
        {
            VariablesScopesStack.Peek().Variables.Add(variable);
        }

        public void EnterNewScope(string scopeIdentifier)
        {
            VariablesScopesStack.Push(new ScopeDefinition(scopeIdentifier));
        }

        public void LeaveScope()
        {
            VariablesScopesStack.Pop();
        }

        public bool IsInScope(IVariableDefinition variable)
        {
            if (variable == null)
            {
                return false;
            }

            return VariablesScopesStack.Any(s => s.Variables.Contains(variable));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.Model.Compiler.Error;

namespace ExampleCodeGenApp.Model
{
    public class VariableReference<T> : ITypedExpression<T>
    {
        public ITypedVariableDefinition<T> LocalVariable { get; set; }

        public string Compile(CompilerContext context)
        {
            if (!context.IsInScope(LocalVariable))
            {
                throw new VariableOutOfScopeException(LocalVariable.VariableName);
            }
            return LocalVariable.VariableName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    public class VariableReference<T> : ITypedExpression<T>
    {
        public LocalVariableDefinition<T> LocalVariable { get; set; }

        public string Compile()
        {
            return LocalVariable.VariableName;
        }
    }
}

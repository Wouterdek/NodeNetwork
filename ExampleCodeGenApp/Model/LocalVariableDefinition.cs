using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    public class LocalVariableDefinition<T> : IVariableDefinition<T>
    {
        public string VariableName { get; private set; }
        public string Value { get; set; }

        public string Compile(CompilerContext context)
        {
            VariableName = context.FindFreeVariableName();
            context.AddVariableToCurrentScope(VariableName);
            return $"local {VariableName} = {Value}\n";
        }
    }
}

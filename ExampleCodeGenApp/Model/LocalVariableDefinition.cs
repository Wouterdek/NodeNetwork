using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    [DataContract]
    public class LocalVariableDefinition<T> : ITypedVariableDefinition<T>
    {
        [DataMember] public string VariableName { get; set; }
        [DataMember] public string Value { get; set; }

        public string Compile(CompilerContext context)
        {
            VariableName = context.FindFreeVariableName();
            context.AddVariableToCurrentScope(this);
            return $"local {VariableName} = {Value}\n";
        }
    }
}

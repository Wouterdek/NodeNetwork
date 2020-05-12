using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExampleCodeGenApp.Model.Compiler.Error
{
    [DataContract]
    public class VariableOutOfScopeException : CompilerException
    {
       [DataMember] public string VariableName { get; set; }

        public VariableOutOfScopeException(string variableName) 
            : base($"The variable '{variableName}' was referenced outside its scope.")
        {
            VariableName = variableName;
        }
    }
}

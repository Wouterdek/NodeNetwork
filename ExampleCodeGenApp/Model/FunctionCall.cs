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
    public class FunctionCall : IStatement
    {
       [DataMember] public string FunctionName { get; set; }
       [DataMember] public List<IExpression> Parameters { get; set; } = new List<IExpression>();

        public string Compile(CompilerContext context)
        {
            return $"{FunctionName}({String.Join(", ", Parameters.Select(p => p.Compile(context)))})\n";
        }
    }
}

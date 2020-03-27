using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    public class StringLiteral : ITypedExpression<string>
    {
        public string Value { get; set; }

        public string Compile(CompilerContext ctx)
        {
            return $"\"{Value}\"";
        }
    }
}

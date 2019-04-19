using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    public class IntLiteral : ITypedExpression<int>
    {
        public int Value { get; set; }

        public string Compile(CompilerContext context)
        {
            return Value.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    public class ForLoop : IStatement
    {
        public IStatement LoopBody { get; set; }
        public IStatement LoopEnd { get; set; }

        public ITypedExpression<int> LowerBound { get; set; }
        public ITypedExpression<int> UpperBound { get; set; }

        public InlineVariableDefinition<int> CurrentIndex { get; } = new InlineVariableDefinition<int>();

        public string Compile(CompilerContext context)
        {
            context.EnterNewScope("For loop");

            CurrentIndex.Value = LowerBound;
            string code = $"for {CurrentIndex.Compile(context)}, {UpperBound.Compile(context)} do\n" +
                   LoopBody.Compile(context) + "\n" +
                   $"end\n" +
                   LoopEnd.Compile(context) + "\n";

            context.LeaveScope();
            return code;
        }
    }
}

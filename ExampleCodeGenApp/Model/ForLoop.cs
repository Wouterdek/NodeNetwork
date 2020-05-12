using System.Runtime.Serialization;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    [DataContract]
    public class ForLoop : IStatement
    {
        [DataMember] public IStatement LoopBody { get; set; }
        [DataMember] public IStatement LoopEnd { get; set; }

        [DataMember] public ITypedExpression<int> LowerBound { get; set; }
        [DataMember] public ITypedExpression<int> UpperBound { get; set; }

        [DataMember] public InlineVariableDefinition<int> CurrentIndex { get; } = new InlineVariableDefinition<int>();

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

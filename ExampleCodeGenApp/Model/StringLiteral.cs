using System.Runtime.Serialization;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    [DataContract]
    public class StringLiteral : ITypedExpression<string>
    {
        [IgnoreDataMember] public string Value { get; set; }

        public string Compile(CompilerContext ctx)
        {
            return $"\"{Value}\"";
        }
    }
}

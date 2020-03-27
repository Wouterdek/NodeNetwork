using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    public class StatementSequence : IStatement
    {
        public List<IStatement> Statements { get; } = new List<IStatement>();

        public StatementSequence()
        {}

        public StatementSequence(IEnumerable<IStatement> statements)
        {
            Statements.AddRange(statements);
        }

        public string Compile(CompilerContext context)
        {
            string result = "";
            foreach (IStatement statement in Statements)
            {
                result += statement.Compile(context);
                result += "\n";
            }
            return result;
        }
    }
}

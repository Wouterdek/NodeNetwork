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
    public class StubStatement : IStatement
    {
        public string Compile(CompilerContext context)
        {
            return "";
        }
    }
}

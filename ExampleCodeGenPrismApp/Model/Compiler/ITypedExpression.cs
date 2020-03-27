using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleCodeGenApp.Model.Compiler
{
    public interface IExpression
    {
        string Compile(CompilerContext context);
    }

    public interface ITypedExpression<T> : IExpression
    {
    }
}

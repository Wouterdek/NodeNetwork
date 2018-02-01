using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    interface IVariableDefinition<T> : IStatement
    {
        string VariableName { get; }
    }
}

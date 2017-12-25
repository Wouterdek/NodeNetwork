using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleShaderEditorApp.Model
{
    public class ShaderFunc
    {
        public Func<string> CompilationFunc { get; set; }

        public ShaderFunc(Func<string> compilationFunc)
        {
            this.CompilationFunc = compilationFunc;
        }

        public string Compile() => CompilationFunc();
    }
}

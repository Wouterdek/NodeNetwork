using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels
{
    public class CodePreviewViewModel : ReactiveObject
    {
        #region Code
        public IStatement Code
        {
            get => _code;
            set => this.RaiseAndSetIfChanged(ref _code, value);
        }
        private IStatement _code;
        #endregion

        #region CompiledCode
        private readonly ObservableAsPropertyHelper<string> _compiledCode;
        public string CompiledCode => _compiledCode.Value; 
        #endregion

        public CodePreviewViewModel()
        {
            this.WhenAnyValue(vm => vm.Code).Where(c => c != null).Select(c => c.Compile(new CompilerContext()))
                .ToProperty(this, vm => vm.CompiledCode, out _compiledCode);
        }
    }
}

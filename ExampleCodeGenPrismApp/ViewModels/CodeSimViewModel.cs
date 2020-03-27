using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;
using MoonSharp.Interpreter;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels
{
    public class CodeSimViewModel : ReactiveObject
    {
        #region Code
        public IStatement Code
        {
            get => _code;
            set => this.RaiseAndSetIfChanged(ref _code, value);
        }
        private IStatement _code;
        #endregion
        
        #region Output
        public string Output
        {
            get => _output;
            set => this.RaiseAndSetIfChanged(ref _output, value);
        }
        private string _output;
        #endregion

        public ReactiveCommand<Unit, Unit> RunScript { get; }
        public ReactiveCommand<Unit, Unit> ClearOutput { get; }

        public CodeSimViewModel()
        {
            RunScript = ReactiveCommand.Create(() =>
                {
                    Script script = new Script();
                    script.Globals["print"] = (Action<string>)Print;
                    string source =  Code.Compile(new CompilerContext());
                    script.DoString(source);
                },
                this.WhenAnyValue(vm => vm.Code).Select(code => code != null));

            ClearOutput = ReactiveCommand.Create(() => { Output = ""; });
        }

        public void Print(string msg)
        {
            Output += msg + "\n";
        }
    }
}

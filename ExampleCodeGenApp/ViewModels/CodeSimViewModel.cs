using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using ExampleCodeGenApp.Model.Compiler;
using MoonSharp.Interpreter;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels
{
    [DataContract]
    public class CodeSimViewModel : ReactiveObject
    {
        #region Code
        [DataMember]
        public IStatement Code
        {
            get => _code;
            set => this.RaiseAndSetIfChanged(ref _code, value);
        }
        [IgnoreDataMember] private IStatement _code;
        #endregion

        #region Output
        [DataMember]
        public string Output
        {
            get => _output;
            set => this.RaiseAndSetIfChanged(ref _output, value);
        }
        [IgnoreDataMember] private string _output;
        #endregion

        [IgnoreDataMember] public ReactiveCommand<Unit, Unit> RunScript { get; }
        [IgnoreDataMember] public ReactiveCommand<Unit, Unit> ClearOutput { get; }

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

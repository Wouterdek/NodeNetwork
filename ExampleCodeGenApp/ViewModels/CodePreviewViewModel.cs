using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.Model.Compiler.Error;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels
{
    [DataContract]
    public class CodePreviewViewModel : ReactiveObject
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

        #region CompilerError
        [DataMember]
        public string CompilerError
        {
            get => _compilerError;
            set => this.RaiseAndSetIfChanged(ref _compilerError, value);
        }
        [IgnoreDataMember] private string _compilerError;
        #endregion

        #region CompiledCode
        [IgnoreDataMember] private readonly ObservableAsPropertyHelper<string> _compiledCode;
        [DataMember] public string CompiledCode => _compiledCode.Value; 
        #endregion

        public CodePreviewViewModel()
        {
            this.WhenAnyValue(vm => vm.Code).Where(c => c != null)
                .Select(c =>
                {
                    CompilerError = "";
                    CompilerContext ctx = new CompilerContext();

                    try
                    {
                        return c.Compile(ctx);
                    }
                    catch (CompilerException e)
                    {
                        string trace = string.Join("\n", ctx.VariablesScopesStack.Select(s => s.Identifier));
                        CompilerError = e.Message + "\nProblem is near:\n"+ trace;
                        return "";
                    }
                })
                .ToProperty(this, vm => vm.CompiledCode, out _compiledCode);
        }
    }
}

using ExampleCodeGenApp.Views.Editors;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels.Editors
{
    public class StringValueEditorViewModel : ValueEditorViewModel<string>
    {
        public StringValueEditorViewModel()
        {
            Value = "";
        }
    }
}

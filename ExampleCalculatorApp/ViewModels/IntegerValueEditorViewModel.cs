using ExampleCalculatorApp.Views;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace ExampleCalculatorApp.ViewModels
{
    public class IntegerValueEditorViewModel : ValueEditorViewModel<int?>
    {
        public IntegerValueEditorViewModel()
        {
            Value = 0;
        }
    }
}

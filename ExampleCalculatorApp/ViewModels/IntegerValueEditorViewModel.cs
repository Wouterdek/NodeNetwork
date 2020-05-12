using ExampleCalculatorApp.Views;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System.Runtime.Serialization;

namespace ExampleCalculatorApp.ViewModels
{
    [DataContract]
    public class IntegerValueEditorViewModel : ValueEditorViewModel<int?>
    {
        static IntegerValueEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new IntegerValueEditorView(), typeof(IViewFor<IntegerValueEditorViewModel>));
        }

        public IntegerValueEditorViewModel()
        {
            Value = 0;
        }
    }
}

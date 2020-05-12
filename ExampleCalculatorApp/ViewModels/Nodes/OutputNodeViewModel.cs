using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.Views;
using ReactiveUI;
using System.Runtime.Serialization;

namespace ExampleCalculatorApp.ViewModels.Nodes
{
    [DataContract]
    public class OutputNodeViewModel : OutputNodeViewModel<int?>, IAmTheOutputViewModel
    {
        static OutputNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<OutputNodeViewModel>));
        }

        public OutputNodeViewModel() : base("Output", new IntegerValueEditorViewModel())
        {
            
        }
    }
}

using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System;

namespace StressTest.ViewModels.Nodes
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
            CanBeRemovedByUser = true;
        }
    }

    [DataContract]
    public class DefaultNodeViewModel : NodeViewModel
    {
        static DefaultNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<DefaultNodeViewModel>));
        }
        public DefaultNodeViewModel()
        {
            Input = new ValueNodeInputViewModel<int?>
            {
                Name = "A"
            };

            Output = new ValueNodeOutputViewModel<int?>
            {
                Name = "B",
                Value = Observable.CombineLatest(Input.ValueChanged, Observable.Return(-1), (i1, i2) => (int?)(i1 ?? i2) + 1)
            };

            Inputs.Add(Input);
            Outputs.Add(Output);
            Output.Value.Subscribe(v => Name = v.ToString());
        }
        [DataMember] public ValueNodeInputViewModel<int?> Input { get; }
        [DataMember] public ValueNodeOutputViewModel<int?> Output { get; }
    }
}

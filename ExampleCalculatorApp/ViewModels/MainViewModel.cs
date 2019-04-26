using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using ExampleCalculatorApp.ViewModels.Nodes;
using ExampleCalculatorApp.Views;
using NodeNetwork;
using NodeNetwork.Toolkit;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCalculatorApp.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        static MainViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new MainWindow(), typeof(IViewFor<MainViewModel>));
        }

        public NodeListViewModel ListViewModel { get; } = new NodeListViewModel();
        public NetworkViewModel NetworkViewModel { get; } = new NetworkViewModel();

        #region ValueLabel
        private string _valueLabel;
        public string ValueLabel
        {
            get => _valueLabel;
            set => this.RaiseAndSetIfChanged(ref _valueLabel, value);
        } 
        #endregion

        public MainViewModel()
        {
            ListViewModel.AddNodeType(() => new SumNodeViewModel());
            ListViewModel.AddNodeType(() => new SubtractionNodeViewModel());
            ListViewModel.AddNodeType(() => new ProductNodeViewModel());
            ListViewModel.AddNodeType(() => new DivisionNodeViewModel());
            ListViewModel.AddNodeType(() => new ConstantNodeViewModel());
            
            OutputNodeViewModel output = new OutputNodeViewModel();
            NetworkViewModel.Nodes.Add(output);

            NetworkViewModel.Validator = network =>
            {
                bool containsLoops = GraphAlgorithms.FindLoops(network).Any();
                if (containsLoops)
                {
                    return new NetworkValidationResult(false, false, new ErrorMessageViewModel("Network contains loops!"));
                }

                bool containsDivisionByZero = GraphAlgorithms.GetConnectedNodesBubbling(output)
                    .OfType<DivisionNodeViewModel>()
                    .Any(n => n.Input2.Value == 0);
                if (containsDivisionByZero)
                {
                    return new NetworkValidationResult(false, true, new ErrorMessageViewModel("Network contains division by zero!"));
                }

                return new NetworkValidationResult(true, true, null);
            };
            
            output.ResultInput.ValueChanged
                .Select(v => (NetworkViewModel.LatestValidation?.IsValid ?? true) ? v.ToString() : "Error")
                .BindTo(this, vm => vm.ValueLabel);
        }
    }
}
